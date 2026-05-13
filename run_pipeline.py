"""
run_pipeline.py — Main ETL Orchestrator for Jobalatica

Usage:
    python run_pipeline.py

Executes the full pipeline:
  1. Read existing DB data
  2. Load all JSON files
  3. Standardize + clean + validate + deduplicate jobs
  4. Normalize skills and generate JobSkills
  5. FK validation
  6. Append into SQLite (Jobs, Skills, JobSkills, DemandSnapshots)
  7. Save cleaned CSVs and validation report
"""

import sys
import time
import logging
import pandas as pd

# ── ensure pipeline/ is importable ────────────────────────────────────────────
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "pipeline"))

from utils import setup_logger, ensure_directories, write_validation_report
from load_json import load_all_json
from clean_jobs import (
    standardize_columns,
    clean_text_fields,
    normalize_experience,
    clean_salaries,
    normalize_currency,
    clean_dates,
    remove_invalid_rows,
    deduplicate,
    fill_source_url,
    extract_countries,
)
from load_sqlite import (
    read_existing_data,
    upsert_canonical_skills,
    insert_jobs,
    insert_jobskills,
    insert_demand_snapshots,
    update_skill_mention_counts,
    save_clean_csvs,
)
from generate_jobskills import generate_jobskills
from validate import validate_jobskills_fk, validate_jobs_columns
from snapshots import generate_demand_snapshots
from config import VALIDATION_REPORT


def run():
    start_time = time.time()
    ensure_directories()
    logger = setup_logger()

    logger.info("=" * 60)
    logger.info("  JOBALATICA ETL PIPELINE — STARTING")
    logger.info("=" * 60)

    stats = {}

    # ──────────────────────────────────────────────────────────────
    # STEP 1 — Read existing DB state
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 1] Reading existing database data …")
    db = read_existing_data()

    # ──────────────────────────────────────────────────────────────
    # STEP 2 — Load all JSON files
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 2] Loading raw JSON files …")
    raw_df = load_all_json()
    stats["Raw rows loaded"] = len(raw_df)

    if raw_df.empty:
        logger.error("No data loaded — aborting pipeline.")
        return

    # ──────────────────────────────────────────────────────────────
    # STEP 3 — Standardize column names
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 3] Standardizing column names …")
    df = standardize_columns(raw_df)

    # ──────────────────────────────────────────────────────────────
    # STEP 4 — Clean text fields
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 4] Cleaning text fields …")
    df = clean_text_fields(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 5 — Normalize experience levels
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 5] Normalizing experience levels …")
    df = normalize_experience(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 4b — Extract Country from Location
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 4b] Extracting country from location …")
    df = extract_countries(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 4c — Synthesise missing SourceUrl
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 4c] Filling missing SourceUrl …")
    df = fill_source_url(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 6 — Clean salaries (parse salary_raw → SalaryMin/Max/Currency)
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 6] Cleaning salary fields …")
    df = clean_salaries(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 7 — Normalize currency
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 7] Normalizing currencies …")
    df = normalize_currency(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 8 & 9 — Clean dates + set IsActive
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 8–9] Cleaning dates and setting IsActive …")
    df = clean_dates(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 10 — Remove invalid rows
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 10] Removing invalid rows …")
    df, rejected_df = remove_invalid_rows(df)
    stats["Rejected rows (validation)"] = len(rejected_df)

    if df.empty:
        logger.error("All rows were rejected — nothing to insert.")
        _finalize(stats, start_time, logger)
        return

    # ──────────────────────────────────────────────────────────────
    # STEP 11 — Deduplicate
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 11] Deduplicating jobs …")
    before_dedup = len(df)
    df = deduplicate(df, db["existing_job_urls"], db["existing_job_keys"])
    stats["Duplicates removed"] = before_dedup - len(df)
    stats["Net new jobs"] = len(df)

    if df.empty:
        logger.info("All jobs are duplicates — nothing new to insert.")
        _finalize(stats, start_time, logger)
        return

    # ──────────────────────────────────────────────────────────────
    # .   NOTE: Salary estimation is NOT part of the main pipeline.
    # .   Run update_existing_estimates.py separately as a
    # .   post-processing layer after the pipeline completes.
    # ──────────────────────────────────────────────────────────────

    # ──────────────────────────────────────────────────────────────
    # STEP 12 — Upsert canonical skills
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 12] Ensuring canonical skills exist in DB …")
    skill_name_to_id = upsert_canonical_skills(db["skill_name_to_id"])
    stats["Skills in DB"] = len(skill_name_to_id)

    # ──────────────────────────────────────────────────────────────
    # STEP 16a — Validate jobs columns & save CSV
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 16a] Preparing jobs for insertion …")
    # Keep raw columns for skill extraction BEFORE stripping them
    jobs_for_skills = df.copy()
    jobs_clean = validate_jobs_columns(df)

    # ──────────────────────────────────────────────────────────────
    # STEP 17a — Insert Jobs into SQLite
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 17a] Inserting jobs into SQLite …")
    new_job_ids = insert_jobs(jobs_clean)
    stats["Jobs inserted"] = len(new_job_ids)

    if not new_job_ids:
        logger.warning("Job insertion returned no IDs — skipping JobSkills.")
        _finalize(stats, start_time, logger)
        return

    # Assign IDs back to the DataFrame for JobSkills generation
    jobs_for_skills = jobs_for_skills.reset_index(drop=True)
    if len(new_job_ids) == len(jobs_for_skills):
        jobs_for_skills["Id"] = new_job_ids
    else:
        # Fallback: re-read from DB by SourceUrl
        from config import DB_PATH
        import sqlite3
        urls = tuple(jobs_clean["SourceUrl"].dropna().tolist())
        if urls:
            with sqlite3.connect(DB_PATH) as conn:
                rows = conn.execute(
                    f"SELECT Id, SourceUrl FROM Jobs WHERE SourceUrl IN ({','.join('?'*len(urls))})",
                    urls
                ).fetchall()
            url_to_id = {r[1]: r[0] for r in rows}
            jobs_for_skills["Id"] = jobs_for_skills["SourceUrl"].map(url_to_id)
        else:
            jobs_for_skills["Id"] = None

    # Drop rows where ID lookup failed
    jobs_for_skills = jobs_for_skills.dropna(subset=["Id"]).copy()
    jobs_for_skills["Id"] = jobs_for_skills["Id"].astype(int)

    valid_job_ids = set(new_job_ids) | db["existing_job_ids"]

    # ──────────────────────────────────────────────────────────────
    # STEP 13 — Generate JobSkills
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 13] Generating JobSkills …")
    jobskills_df = generate_jobskills(
        jobs_for_skills,
        skill_name_to_id,
        db["existing_jobskills"]
    )

    # ──────────────────────────────────────────────────────────────
    # STEP 14 — FK validation
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 14] Validating foreign keys …")
    jobskills_df, fk_rejected = validate_jobskills_fk(
        jobskills_df, valid_job_ids, set(skill_name_to_id.values())
    )
    stats["JobSkills FK rejected"] = len(fk_rejected)
    stats["JobSkills to insert"] = len(jobskills_df)

    # ──────────────────────────────────────────────────────────────
    # STEP 17b — Insert JobSkills
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 17b] Inserting JobSkills into SQLite …")
    insert_jobskills(jobskills_df)

    # ──────────────────────────────────────────────────────────────
    # Update TotalJobMentions
    # ──────────────────────────────────────────────────────────────
    logger.info("[POST] Updating TotalJobMentions counts …")
    update_skill_mention_counts()

    # ──────────────────────────────────────────────────────────────
    # STEP 15 — Generate and insert DemandSnapshots
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 15] Generating DemandSnapshots …")
    snapshots_df = generate_demand_snapshots(jobs_clean)
    insert_demand_snapshots(snapshots_df)
    stats["DemandSnapshots generated"] = len(snapshots_df)

    # ──────────────────────────────────────────────────────────────
    # STEP 16b — Save cleaned CSVs
    # ──────────────────────────────────────────────────────────────
    logger.info("[STEP 16b] Saving cleaned CSV exports …")
    save_clean_csvs(jobs_clean, jobskills_df)

    _finalize(stats, start_time, logger)


def _finalize(stats: dict, start_time: float, logger: logging.Logger):
    elapsed = round(time.time() - start_time, 2)
    stats["Execution time (s)"] = elapsed

    logger.info("=" * 60)
    logger.info("  PIPELINE COMPLETE")
    for k, v in stats.items():
        logger.info(f"  {k:<35}: {v}")
    logger.info(f"  Execution time                    : {elapsed}s")
    logger.info("=" * 60)

    write_validation_report(VALIDATION_REPORT, stats)


if __name__ == "__main__":
    run()

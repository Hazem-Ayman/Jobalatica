"""
load_sqlite.py — STEPS 1 & 16–17: Read from and write to SQLite
"""

import logging
import sqlite3
import pandas as pd
from sqlalchemy import create_engine, text

from config import (
    DB_PATH, CANONICAL_SKILLS, SKILL_CATEGORY_MAP,
    CLEAN_JOBS_CSV, CLEAN_JOBSKILLS_CSV
)

logger = logging.getLogger("jobalatica_etl")


def _get_engine():
    return create_engine(f"sqlite:///{DB_PATH}")


# ─── STEP 1: Read existing DB data ───────────────────────────────────────────

def read_existing_data() -> dict:
    """
    Read Jobs, Skills, and JobSkills from the existing database.
    Returns a dict with sets/dicts for deduplication and skill reuse.
    """
    engine = _get_engine()
    result = {
        "existing_job_urls":  set(),
        "existing_job_keys":  set(),
        "existing_job_ids":   set(),
        "skill_name_to_id":   {},
        "existing_skill_ids": set(),
        "existing_jobskills": set(),
    }

    try:
        jobs_df = pd.read_sql("SELECT Id, Title, Company, Location, SourceUrl FROM Jobs", engine)
        result["existing_job_urls"] = set(jobs_df["SourceUrl"].dropna().str.strip())
        result["existing_job_ids"]  = set(jobs_df["Id"])
        result["existing_job_keys"] = set(
            (jobs_df["Title"].str.lower().str.strip() + "||" +
             jobs_df["Company"].str.lower().str.strip() + "||" +
             jobs_df["Location"].str.lower().str.strip())
        )
        logger.info(f"Existing jobs in DB: {len(jobs_df):,}")
    except Exception as e:
        logger.warning(f"Could not read Jobs table: {e}")

    try:
        skills_df = pd.read_sql("SELECT Id, Name FROM Skills", engine)
        result["skill_name_to_id"] = dict(
            zip(skills_df["Name"], skills_df["Id"])
        )
        result["existing_skill_ids"] = set(skills_df["Id"])
        logger.info(f"Existing skills in DB: {len(skills_df):,}")
    except Exception as e:
        logger.warning(f"Could not read Skills table: {e}")

    try:
        js_df = pd.read_sql("SELECT JobId, SkillId FROM JobSkills", engine)
        result["existing_jobskills"] = set(
            zip(js_df["JobId"], js_df["SkillId"])
        )
        logger.info(f"Existing JobSkills in DB: {len(js_df):,}")
    except Exception as e:
        logger.warning(f"Could not read JobSkills table: {e}")

    return result


# ─── Ensure canonical skills exist in DB ─────────────────────────────────────

def upsert_canonical_skills(skill_name_to_id: dict) -> dict:
    """
    Ensure all CANONICAL_SKILLS are present in the Skills table.
    Inserts only missing ones. Returns updated skill_name_to_id mapping.
    """
    engine = _get_engine()
    new_skills = []
    for name in CANONICAL_SKILLS:
        if name not in skill_name_to_id:
            category = SKILL_CATEGORY_MAP.get(name, "Other")
            new_skills.append({
                "Name": name,
                "Category": category,
                "TotalJobMentions": 0
            })

    if new_skills:
        new_df = pd.DataFrame(new_skills)
        new_df.to_sql("Skills", engine, if_exists="append", index=False)
        logger.info(f"Inserted {len(new_skills)} new canonical skills into Skills table")

        # Re-read to get assigned IDs
        skills_df = pd.read_sql("SELECT Id, Name FROM Skills", engine)
        skill_name_to_id = dict(zip(skills_df["Name"], skills_df["Id"]))
    else:
        logger.info("All canonical skills already present in Skills table")

    return skill_name_to_id


# ─── STEP 17: Insert Jobs ────────────────────────────────────────────────────

def insert_jobs(jobs_df: pd.DataFrame) -> list[int]:
    """
    Append clean jobs into the Jobs table.
    Returns list of newly inserted row IDs.
    """
    if jobs_df.empty:
        logger.info("No new jobs to insert.")
        return []

    engine = _get_engine()
    try:
        jobs_df.to_sql("Jobs", engine, if_exists="append", index=False)
        logger.info(f"Inserted {len(jobs_df):,} jobs into Jobs table")

        # Retrieve newly inserted IDs by SourceUrl
        urls = tuple(jobs_df["SourceUrl"].dropna().tolist())
        if not urls:
            return []
        placeholder = ",".join("?" * len(urls))
        with sqlite3.connect(DB_PATH) as conn:
            rows = conn.execute(
                f"SELECT Id FROM Jobs WHERE SourceUrl IN ({placeholder})", urls
            ).fetchall()
        return [r[0] for r in rows]

    except Exception as e:
        logger.error(f"Error inserting jobs: {e}")
        return []


# ─── Insert JobSkills ────────────────────────────────────────────────────────

def insert_jobskills(jobskills_df: pd.DataFrame):
    """Append validated JobSkills rows."""
    if jobskills_df.empty:
        logger.info("No new JobSkills to insert.")
        return

    engine = _get_engine()
    try:
        jobskills_df.to_sql("JobSkills", engine, if_exists="append", index=False)
        logger.info(f"Inserted {len(jobskills_df):,} rows into JobSkills table")
    except Exception as e:
        logger.error(f"Error inserting JobSkills: {e}")


# ─── Insert DemandSnapshots ──────────────────────────────────────────────────

def insert_demand_snapshots(snapshots_df: pd.DataFrame):
    """Append DemandSnapshot aggregation rows."""
    if snapshots_df.empty:
        logger.info("No DemandSnapshots to insert.")
        return

    engine = _get_engine()
    try:
        snapshots_df.to_sql("DemandSnapshots", engine, if_exists="append", index=False)
        logger.info(f"Inserted {len(snapshots_df):,} rows into DemandSnapshots table")
    except Exception as e:
        logger.error(f"Error inserting DemandSnapshots: {e}")


# ─── Update TotalJobMentions on Skills ───────────────────────────────────────

def update_skill_mention_counts():
    """Recalculate TotalJobMentions for every skill based on JobSkills count."""
    try:
        with sqlite3.connect(DB_PATH) as conn:
            conn.execute("""
                UPDATE Skills
                SET TotalJobMentions = (
                    SELECT COUNT(*) FROM JobSkills
                    WHERE JobSkills.SkillId = Skills.Id
                )
            """)
            conn.commit()
        logger.info("Updated TotalJobMentions for all skills")
    except Exception as e:
        logger.error(f"Error updating TotalJobMentions: {e}")


# ─── STEP 16: Save cleaned CSV exports ───────────────────────────────────────

def save_clean_csvs(jobs_df: pd.DataFrame, jobskills_df: pd.DataFrame):
    """Export cleaned data as CSVs for debugging and auditability."""
    if not jobs_df.empty:
        jobs_df.to_csv(CLEAN_JOBS_CSV, index=False, encoding="utf-8")
        logger.info(f"Saved clean_jobs.csv → {CLEAN_JOBS_CSV}")

    if not jobskills_df.empty:
        jobskills_df.to_csv(CLEAN_JOBSKILLS_CSV, index=False, encoding="utf-8")
        logger.info(f"Saved clean_jobskills.csv → {CLEAN_JOBSKILLS_CSV}")

"""
validate.py — STEP 14: Foreign Key validation before DB insertion
"""

import logging
import pandas as pd

logger = logging.getLogger("jobalatica_etl")


def validate_jobskills_fk(
    jobskills_df: pd.DataFrame,
    valid_job_ids: set,
    valid_skill_ids: set
) -> tuple[pd.DataFrame, pd.DataFrame]:
    """
    Validate that every row in jobskills_df has valid FK references.

    Returns (valid_df, rejected_df)
    """
    if jobskills_df.empty:
        return jobskills_df, pd.DataFrame()

    valid_job_mask   = jobskills_df["JobId"].isin(valid_job_ids)
    valid_skill_mask = jobskills_df["SkillId"].isin(valid_skill_ids)
    valid_mask = valid_job_mask & valid_skill_mask

    valid_df    = jobskills_df[valid_mask].copy()
    rejected_df = jobskills_df[~valid_mask].copy()

    if not rejected_df.empty:
        logger.warning(
            f"FK validation: {len(rejected_df):,} JobSkill rows rejected "
            f"(orphan FK references)"
        )

    logger.info(
        f"FK validation passed: {len(valid_df):,} / {len(jobskills_df):,} JobSkill rows"
    )
    return valid_df, rejected_df


def validate_jobs_columns(df: pd.DataFrame) -> pd.DataFrame:
    """
    Ensure the jobs DataFrame has exactly the columns needed for DB insertion
    and drop any helper columns.
    """
    REQUIRED_DB_COLS = [
        "Title", "Company", "Location", "Country",
        "SalaryMin", "SalaryMax", "Currency",
        "ExperienceLevel", "SourceUrl", "SourceSite",
        "PostedAt", "ScrapedAt", "IsActive"
    ]

    helper_cols = [c for c in df.columns if c.startswith("_") or
                   c in ("skills_raw", "description_raw")]
    df = df.drop(columns=helper_cols, errors="ignore")

    # Add missing DB columns as NULL
    for col in REQUIRED_DB_COLS:
        if col not in df.columns:
            df[col] = None

    return df[REQUIRED_DB_COLS]

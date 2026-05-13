"""
generate_jobskills.py — STEP 13: Generate JobSkills rows from cleaned jobs
"""

import logging
import pandas as pd
from normalize_skills import extract_normalized_skills

logger = logging.getLogger("jobalatica_etl")


def generate_jobskills(
    jobs_df: pd.DataFrame,
    skill_name_to_id: dict[str, int],
    existing_jobskills: set[tuple]
) -> pd.DataFrame:
    """
    For every job in jobs_df, extract its canonical skills and build
    JobSkills rows.

    Parameters
    ----------
    jobs_df              : DataFrame with at least columns [Id, skills_raw, description_raw, Title]
    skill_name_to_id     : mapping { canonical_name -> skill_id }
    existing_jobskills   : set of (JobId, SkillId) tuples already in DB

    Returns
    -------
    DataFrame with columns [JobId, SkillId]
    """
    records = []
    unmatched_skills: set[str] = set()

    for _, row in jobs_df.iterrows():
        job_id = row.get("Id")
        if pd.isna(job_id):
            continue
        job_id = int(job_id)

        canonical_skills = extract_normalized_skills(row)

        for skill_name in canonical_skills:
            skill_id = skill_name_to_id.get(skill_name)
            if skill_id is None:
                unmatched_skills.add(skill_name)
                continue

            pair = (job_id, skill_id)
            if pair in existing_jobskills:
                continue  # already exists in DB

            records.append({"JobId": job_id, "SkillId": skill_id})
            existing_jobskills.add(pair)  # prevent internal duplicates

    if unmatched_skills:
        logger.warning(
            f"Skills referenced but not found in Skills table "
            f"({len(unmatched_skills)}): {sorted(unmatched_skills)[:20]}"
        )

    jobskills_df = pd.DataFrame(records, columns=["JobId", "SkillId"])
    jobskills_df = jobskills_df.drop_duplicates()

    logger.info(f"Generated {len(jobskills_df):,} new JobSkill rows")
    return jobskills_df.reset_index(drop=True)

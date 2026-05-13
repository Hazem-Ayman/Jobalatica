"""
estimate_salaries.py — STEP 15b: Calculate salary estimates for jobs with hidden salaries
based on Title and ExperienceLevel averages.
"""

import logging
import pandas as pd
import re

logger = logging.getLogger("jobalatica_etl")

def _normalize_title_for_grouping(title: str) -> str:
    if not title: return ""
    t = title.lower()
    # Remove common fluff
    t = re.sub(r"\(.*?\)", "", t)
    t = re.sub(r"\b(senior|junior|mid|lead|jr|sr|staff|principal|head of|intern)\b", "", t)
    t = re.sub(r"[^a-z+ ]", " ", t)
    t = " ".join(t.split())
    return t

def calculate_estimates(df: pd.DataFrame, existing_df: pd.DataFrame = None) -> pd.DataFrame:
    """
    1. Build a lookup table of average salaries per (NormalizedTitle, ExperienceLevel).
    2. Fill EstimatedSalary fields for rows with missing SalaryMin.
    """
    # Combine current batch with existing data for better averages
    full_df = df.copy()
    if existing_df is not None and not existing_df.empty:
        full_df = pd.concat([full_df, existing_df], ignore_index=True)

    # Prepare data for averaging
    # Convert salary columns to numeric just in case
    full_df["SalaryMin"] = pd.to_numeric(full_df["SalaryMin"], errors="coerce")
    full_df["SalaryMax"] = pd.to_numeric(full_df["SalaryMax"], errors="coerce")
    
    has_salary = full_df[full_df["SalaryMin"].notna()].copy()
    
    if has_salary.empty:
        logger.warning("No salary data available in batch or DB to calculate estimates.")
        df["IsSalaryEstimated"] = False
        df["EstimatedSalaryMin"] = None
        df["EstimatedSalaryMax"] = None
        return df

    has_salary["_norm_title"] = has_salary["Title"].apply(_normalize_title_for_grouping)
    
    # Group by normalized title and level
    stats = has_salary.groupby(["_norm_title", "ExperienceLevel"]).agg({
        "SalaryMin": "mean",
        "SalaryMax": "mean"
    }).reset_index()
    
    stats_dict = {}
    for _, row in stats.iterrows():
        stats_dict[(row["_norm_title"], row["ExperienceLevel"])] = (row["SalaryMin"], row["SalaryMax"])

    # Apply to current batch
    def _apply_estimate(row):
        if pd.notna(row["SalaryMin"]):
            return False, None, None
        
        norm_t = _normalize_title_for_grouping(row["Title"])
        level = row["ExperienceLevel"]
        
        estimate = stats_dict.get((norm_t, level))
        if estimate:
            return True, round(estimate[0], 2), round(estimate[1], 2)
        
        # Fallback: ignore level, just use title
        title_only_stats = stats[stats["_norm_title"] == norm_t]
        if not title_only_stats.empty:
            avg_min = title_only_stats["SalaryMin"].mean()
            avg_max = title_only_stats["SalaryMax"].mean()
            return True, round(avg_min, 2), round(avg_max, 2)
            
        return False, None, None

    estimates = df.apply(_apply_estimate, axis=1)
    df["IsSalaryEstimated"]  = [e[0] for e in estimates]
    df["EstimatedSalaryMin"] = [e[1] for e in estimates]
    df["EstimatedSalaryMax"] = [e[2] for e in estimates]

    est_count = df["IsSalaryEstimated"].sum()
    logger.info(f"Calculated salary estimates for {est_count:,} jobs.")
    
    return df

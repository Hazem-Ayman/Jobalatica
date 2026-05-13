"""
snapshots.py — STEP 15: Generate DemandSnapshot aggregations
"""

import logging
import pandas as pd
from datetime import date

logger = logging.getLogger("jobalatica_etl")


def generate_demand_snapshots(jobs_df: pd.DataFrame) -> pd.DataFrame:
    """
    Aggregate inserted jobs into DemandSnapshot rows.

    Groups by (Title, Location) and computes:
      - PostingCount
      - AvgSalaryMin
      - AvgSalaryMax

    SnapshotDate is today's date (UTC).
    """
    if jobs_df.empty:
        logger.info("No jobs to snapshot.")
        return pd.DataFrame()

    snapshot_date = date.today().isoformat()

    group_df = jobs_df.copy()
    group_df["SalaryMin"] = pd.to_numeric(group_df.get("SalaryMin"), errors="coerce")
    group_df["SalaryMax"] = pd.to_numeric(group_df.get("SalaryMax"), errors="coerce")

    agg = (
        group_df
        .groupby(["Title", "Location"], dropna=True)
        .agg(
            PostingCount=("Title", "count"),
            AvgSalaryMin=("SalaryMin", "mean"),
            AvgSalaryMax=("SalaryMax", "mean"),
        )
        .reset_index()
    )

    agg["SnapshotDate"] = snapshot_date

    # Round decimals
    agg["AvgSalaryMin"] = agg["AvgSalaryMin"].round(2)
    agg["AvgSalaryMax"] = agg["AvgSalaryMax"].round(2)

    # Fill NULL salary averages with 0.0 (schema requires NOT NULL)
    agg["AvgSalaryMin"] = agg["AvgSalaryMin"].fillna(0.0)
    agg["AvgSalaryMax"] = agg["AvgSalaryMax"].fillna(0.0)

    # Rename to match DB schema
    agg = agg.rename(columns={
        "Title":    "JobTitle",
        "Location": "Location",
    })

    logger.info(f"Generated {len(agg):,} DemandSnapshot rows")
    return agg[["JobTitle", "Location", "PostingCount",
                "AvgSalaryMin", "AvgSalaryMax", "SnapshotDate"]]

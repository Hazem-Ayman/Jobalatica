"""
update_existing_estimates.py — One-time script to calculate salary estimates for 
all jobs currently in the database that have missing salaries.
"""

import sys
import os
import pandas as pd
import sqlite3
import logging

# Add pipeline to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "pipeline"))

from estimate_salaries import calculate_estimates
from config import DB_PATH

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("update_estimates")

def run():
    logger.info(f"Connecting to database: {DB_PATH}")
    conn = sqlite3.connect(DB_PATH)
    
    # Load all jobs — only those that do NOT already have an estimate
    # (never recalculate existing backend salary values)
    df = pd.read_sql("SELECT * FROM Jobs WHERE EstimatedSalaryMin IS NULL", conn)
    logger.info(f"Loaded {len(df):,} jobs without existing estimates.")
    
    if df.empty:
        logger.info("No jobs need estimation — all already have estimates or real salaries.")
        conn.close()
        return
    
    df = calculate_estimates(df)
    
    est_count = df["IsSalaryEstimated"].sum()
    logger.info(f"New estimates calculated for {est_count:,} jobs.")
    
    if est_count > 0:
        logger.info("Updating database...")
        cursor = conn.cursor()
        
        updates = []
        for _, row in df[df["IsSalaryEstimated"]].iterrows():
            updates.append((
                1, # IsSalaryEstimated = True
                row["EstimatedSalaryMin"],
                row["EstimatedSalaryMax"],
                row["Id"]
            ))
        
        cursor.executemany("""
            UPDATE Jobs 
            SET IsSalaryEstimated = ?, 
                EstimatedSalaryMin = ?, 
                EstimatedSalaryMax = ? 
            WHERE Id = ?
        """, updates)
        
        conn.commit()
        logger.info("Database updated successfully.")
    
    conn.close()

if __name__ == "__main__":
    run()

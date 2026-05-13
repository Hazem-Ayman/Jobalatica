"""
load_json.py — STEP 2: Load and merge all raw JSON job files
"""

import os
import json
import logging
import pandas as pd
from config import RAW_DIR

logger = logging.getLogger("jobalatica_etl")


def _load_single_file(filepath: str) -> pd.DataFrame:
    """Load one JSON file, supporting both list and dict structures."""
    try:
        with open(filepath, "r", encoding="utf-8") as f:
            data = json.load(f)

        if isinstance(data, list):
            df = pd.DataFrame(data)
        elif isinstance(data, dict):
            # Some scrapers wrap jobs in a key like "jobs" or "results"
            for key in ("jobs", "results", "data", "items"):
                if key in data and isinstance(data[key], list):
                    df = pd.DataFrame(data[key])
                    break
            else:
                df = pd.DataFrame([data])
        else:
            logger.warning(f"Unknown JSON structure in {filepath}, skipping.")
            return pd.DataFrame()

        df["_source_file"] = os.path.basename(filepath)
        logger.debug(f"  Loaded {len(df):,} rows from {os.path.basename(filepath)}")
        return df

    except (json.JSONDecodeError, ValueError) as e:
        logger.error(f"  Failed to parse {filepath}: {e}")
        return pd.DataFrame()


def load_all_json() -> pd.DataFrame:
    """
    Read every .json file in RAW_DIR and return a single merged DataFrame.
    """
    if not os.path.isdir(RAW_DIR):
        logger.error(f"Raw data directory not found: {RAW_DIR}")
        return pd.DataFrame()

    json_files = sorted(
        f for f in os.listdir(RAW_DIR) if f.lower().endswith(".json")
    )

    if not json_files:
        logger.error(f"No JSON files found in {RAW_DIR}")
        return pd.DataFrame()

    logger.info(f"Found {len(json_files)} JSON file(s) in raw/")

    frames = []
    for filename in json_files:
        filepath = os.path.join(RAW_DIR, filename)
        frames.append(_load_single_file(filepath))

    combined = pd.concat(frames, ignore_index=True)
    logger.info(f"Total raw rows loaded: {len(combined):,}")
    return combined

"""
utils.py — Shared utilities for the Jobalatica ETL Pipeline
"""

import os
import logging
from datetime import datetime
from config import LOG_DIR, LOG_FILE, CLEANED_DIR


def ensure_directories():
    """Create all required output directories if they don't exist."""
    for directory in [LOG_DIR, CLEANED_DIR]:
        os.makedirs(directory, exist_ok=True)


def setup_logger() -> logging.Logger:
    """Configure and return the pipeline logger."""
    ensure_directories()
    logger = logging.getLogger("jobalatica_etl")
    logger.setLevel(logging.DEBUG)

    if logger.handlers:
        return logger

    fmt = logging.Formatter(
        "[%(asctime)s] [%(levelname)-8s] %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S"
    )

    # File handler
    fh = logging.FileHandler(LOG_FILE, encoding="utf-8")
    fh.setLevel(logging.DEBUG)
    fh.setFormatter(fmt)

    # Console handler
    ch = logging.StreamHandler()
    ch.setLevel(logging.INFO)
    ch.setFormatter(fmt)

    logger.addHandler(fh)
    logger.addHandler(ch)
    return logger


def write_validation_report(path: str, stats: dict):
    """Write a plain-text validation report."""
    with open(path, "w", encoding="utf-8") as f:
        f.write("=" * 60 + "\n")
        f.write("  JOBALATICA ETL PIPELINE — VALIDATION REPORT\n")
        f.write("=" * 60 + "\n")
        f.write(f"  Generated at : {datetime.utcnow().isoformat()} UTC\n")
        f.write("=" * 60 + "\n\n")
        for key, value in stats.items():
            f.write(f"  {key:<30}: {value}\n")
        f.write("\n" + "=" * 60 + "\n")

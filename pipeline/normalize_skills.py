"""
normalize_skills.py — STEP 12: Normalize skill aliases to canonical names
"""

import re
import logging
import pandas as pd
from config import SKILL_ALIAS_MAP, CANONICAL_SKILLS

logger = logging.getLogger("jobalatica_etl")


def _extract_skill_tokens(value) -> list[str]:
    """
    Extract individual skill tokens from a raw skills field.
    Handles: comma-separated strings, JSON-like lists, pipe-separated, etc.
    """
    if pd.isna(value) or value is None:
        return []
    text = str(value).strip()

    # Remove brackets/quotes that come from JSON serialization
    text = re.sub(r"[\[\]\"']", "", text)

    # Split on common delimiters
    tokens = re.split(r"[,;|/\n\r]+", text)
    return [t.strip() for t in tokens if t.strip()]


def _extract_from_description(description: str) -> list[str]:
    """
    Scan a job description text for known canonical skills and aliases.
    Returns a list of matched canonical skill names.
    """
    if not description or pd.isna(description):
        return []

    found = set()
    text_lower = description.lower()

    for alias, canonical in SKILL_ALIAS_MAP.items():
        # Use word-boundary matching to avoid false positives
        pattern = r"(?<![a-z0-9\-\+#])" + re.escape(alias) + r"(?![a-z0-9\-\+#])"
        if re.search(pattern, text_lower):
            found.add(canonical)

    return list(found)


def normalize_skill_token(token: str) -> str | None:
    """Map a single raw skill token to its canonical name (or None)."""
    if not token:
        return None
    cleaned = token.strip().lower()
    return SKILL_ALIAS_MAP.get(cleaned)


def extract_normalized_skills(row: pd.Series) -> list[str]:
    """
    Given a DataFrame row, extract a deduplicated list of canonical skill names
    from both the 'skills_raw' field and the 'description_raw' field.
    """
    found = set()

    # From explicit skills/tags field
    raw_field = row.get("skills_raw")
    for token in _extract_skill_tokens(raw_field):
        canonical = normalize_skill_token(token)
        if canonical:
            found.add(canonical)

    # From job description
    desc_field = row.get("description_raw")
    if desc_field and not pd.isna(desc_field):
        for canonical in _extract_from_description(str(desc_field)):
            found.add(canonical)

    # Also scan Title for skill hints
    title = row.get("Title", "")
    if title and not pd.isna(title):
        for canonical in _extract_from_description(str(title)):
            found.add(canonical)

    return sorted(found)

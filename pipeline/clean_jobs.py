"""
clean_jobs.py — STEPS 3–11: Standardize, clean, validate, and deduplicate jobs

Handles the actual raw data schemas found in seeddata/raw/:
  - salary_raw  → SalaryMin, SalaryMax, Currency  (e.g. "$1,000 - $1,500", "1000 - 10000 EGP")
  - experience_raw → ExperienceLevel              (e.g. "Management·5-15 Years of Experience")
  - Country extracted from Location string
  - SourceUrl synthesised from SourceSite + job_id_raw when missing
  - PostedAt falls back to ScrapedAt when relative/missing
"""

import re
import logging
import pandas as pd
from datetime import datetime, timezone
from dateutil import parser as date_parser

from config import (
    COLUMN_MAP, LEVEL_MAP, REQUIRED_FIELDS,
    VALID_CURRENCIES, REJECTED_JOBS_CSV
)

logger = logging.getLogger("jobalatica_etl")

# ─── Known country keywords ───────────────────────────────────────────────────
COUNTRY_KEYWORDS = {
    "egypt": "Egypt", "cairo": "Egypt", "giza": "Egypt",
    "alexandria": "Egypt", "maadi": "Egypt", "nasr city": "Egypt",
    "heliopolis": "Egypt", "zamalek": "Egypt", "6th october": "Egypt",
    "new cairo": "Egypt", "10th ramadan": "Egypt",

    "uae": "UAE", "dubai": "UAE", "abu dhabi": "UAE", "sharjah": "UAE",
    "united arab emirates": "UAE",

    "saudi": "Saudi Arabia", "riyadh": "Saudi Arabia", "jeddah": "Saudi Arabia",
    "ksa": "Saudi Arabia",

    "kuwait": "Kuwait", "qatar": "Qatar", "doha": "Qatar",
    "bahrain": "Bahrain", "oman": "Oman", "muscat": "Oman",

    "jordan": "Jordan", "amman": "Jordan",
    "lebanon": "Lebanon", "beirut": "Lebanon",

    "usa": "USA", "new york": "USA", "california": "USA",
    "texas": "USA", "united states": "USA",

    "uk": "UK", "london": "UK", "united kingdom": "UK",
    "germany": "Germany", "berlin": "Germany",
    "canada": "Canada", "toronto": "Canada",
    "india": "India", "bangalore": "India", "mumbai": "India",
}

# ─── Currency symbols / codes inside salary strings ───────────────────────────
CURRENCY_PATTERNS = [
    (r"\$",       "USD"),
    (r"usd",      "USD"),
    (r"€|eur",    "EUR"),
    (r"£|gbp",    "GBP"),
    (r"egp",      "EGP"),
    (r"aed",      "AED"),
    (r"sar",      "SAR"),
    (r"kwd",      "KWD"),
    (r"qar",      "QAR"),
    (r"inr|₹",    "INR"),
    (r"cad",      "CAD"),
    (r"aud",      "AUD"),
]

# ─── STEP 3: Standardize Column Names ────────────────────────────────────────

def standardize_columns(df: pd.DataFrame) -> pd.DataFrame:
    """
    Rename raw scraped columns to schema-compliant names.
    First match in COLUMN_MAP wins.  After renaming, duplicate column names
    (e.g. post_date + date_posted both → PostedAt) are coalesced into one
    column by taking the first non-null value per row.
    """
    df_cols_lower = {c.lower(): c for c in df.columns}
    rename = {}

    for raw_key, schema_key in COLUMN_MAP.items():
        orig = df_cols_lower.get(raw_key.lower())
        if orig and orig not in rename:
            rename[orig] = schema_key

    df = df.rename(columns=rename)

    # Coalesce any duplicate column names produced by the rename
    if df.columns.duplicated().any():
        seen = {}
        new_df = pd.DataFrame(index=df.index)
        for col in df.columns:
            if col not in seen:
                seen[col] = True
                # Gather all columns with this name
                dupes = df.loc[:, [c == col for c in df.columns]]
                if isinstance(dupes, pd.DataFrame) and dupes.shape[1] > 1:
                    # Take first non-null/non-empty value across duplicate columns
                    def _coalesce(row):
                        for v in row:
                            if v is not None and not (isinstance(v, float) and pd.isna(v)) and str(v).strip():
                                return v
                        return None
                    new_df[col] = dupes.apply(_coalesce, axis=1)
                else:
                    new_df[col] = df[col]
        df = new_df

    # Handle scraped_at: rename to _scraped_at_raw (fallback for PostedAt)
    if "scraped_at" in df.columns and "PostedAt" not in df.columns:
        df = df.rename(columns={"scraped_at": "PostedAt"})
    elif "scraped_at" in df.columns:
        df = df.rename(columns={"scraped_at": "_scraped_at_raw"})

    logger.debug(f"Column rename applied: {rename}")
    return df


# ─── STEP 4: Clean Text Fields ────────────────────────────────────────────────

def _clean_text(value) -> str | None:
    if pd.isna(value) or value is None:
        return None
    text = str(value)
    text = re.sub(r"[\x00-\x1f\x7f-\x9f]", " ", text)
    text = re.sub(r"\s+", " ", text).strip()
    return text if text else None


def clean_text_fields(df: pd.DataFrame) -> pd.DataFrame:
    for col in ["Title", "Company", "Location"]:
        if col in df.columns:
            df[col] = df[col].apply(_clean_text)
    return df


# ─── Country extraction from Location ────────────────────────────────────────

def _extract_country(location: str | None) -> str | None:
    """Try to derive a country from a free-text location string."""
    if not location or pd.isna(location):
        return None
    loc_lower = location.lower()
    # Direct suffix check (e.g. "Giza, El Omraniya, Egypt")
    parts = [p.strip() for p in re.split(r"[,|/\n]", loc_lower)]
    last = parts[-1] if parts else ""
    if last in COUNTRY_KEYWORDS:
        return COUNTRY_KEYWORDS[last]
    # Substring scan
    for keyword, country in COUNTRY_KEYWORDS.items():
        if keyword in loc_lower:
            return country
    return None


def extract_countries(df: pd.DataFrame) -> pd.DataFrame:
    """Fill the Country column: use existing value or extract from Location."""
    if "Country" not in df.columns:
        df["Country"] = None
    missing = df["Country"].isna() | (df["Country"].astype(str).str.strip() == "")
    df.loc[missing, "Country"] = df.loc[missing, "Location"].apply(_extract_country)
    return df


# ─── STEP 5: Normalize Experience Levels ─────────────────────────────────────

def _infer_level_from_title(title: str | None) -> str | None:
    """Scan job title for experience hints as a last resort."""
    if not title:
        return None
    t = title.lower()
    if any(k in t for k in ("junior", "jr ", "entry", "graduate", "intern", "fresher")):
        return "Entry"
    if any(k in t for k in ("senior", "sr ", "lead ", "principal", "architect", "head ", "director")):
        return "Senior"
    if any(k in t for k in ("staff", "manager", " vp ", "chief")):
        return "Lead"
    return None


def _normalize_level(val, title=None) -> str | None:
    if not pd.isna(val) and val is not None:
        text = str(val).strip().lower()
        # Direct map
        if text in LEVEL_MAP:
            return LEVEL_MAP[text]
        # Substring search (e.g. "Management·5-15 Years of Experience")
        for key, mapped in LEVEL_MAP.items():
            if key in text:
                return mapped
    # Fall back to title inference
    if title:
        inferred = _infer_level_from_title(str(title))
        if inferred:
            return inferred
    return None


def normalize_experience(df: pd.DataFrame) -> pd.DataFrame:
    """
    Populate ExperienceLevel from:
    1. ExperienceLevel column (if already mapped)
    2. experience_raw column
    3. Inference from job Title
    """
    if "ExperienceLevel" not in df.columns:
        df["ExperienceLevel"] = None
    if "experience_raw" not in df.columns:
        df["experience_raw"] = None

    def _apply(row):
        # Already a valid mapped value?
        current = row.get("ExperienceLevel")
        if current in ("Entry", "Mid", "Senior", "Lead"):
            return current
        # Try LEVEL_MAP on ExperienceLevel raw string
        result = _normalize_level(current, row.get("Title"))
        if result:
            return result
        # Try experience_raw field
        result = _normalize_level(row.get("experience_raw"), row.get("Title"))
        if result:
            return result
        return None   # will be rejected unless we want to default

    df["ExperienceLevel"] = df.apply(_apply, axis=1)
    return df


# ─── STEP 6 & 7: Salary + Currency from salary_raw ───────────────────────────

def _detect_currency(text: str) -> str:
    """Detect currency from a salary string."""
    t = text.lower()
    for pattern, code in CURRENCY_PATTERNS:
        if re.search(pattern, t):
            return code
    return "USD"   # default


def _parse_salary_value(text: str) -> float | None:
    """Extract a numeric salary from a string fragment."""
    text = re.sub(r"[$€£₹¥,\s]", "", text.lower())
    match_k = re.match(r"^(\d+(?:\.\d+)?)k$", text)
    if match_k:
        return float(match_k.group(1)) * 1000
    match = re.search(r"(\d+(?:\.\d+)?)", text)
    if match:
        return float(match.group(1))
    return None


def _split_salary_range(raw) -> tuple[float | None, float | None, str]:
    """
    Parse combined salary strings into (SalaryMin, SalaryMax, Currency).
    Handles: "$1,000 - $1,500", "1000 - 10000 EGP", "N/A", "5k"
    """
    if pd.isna(raw) or raw is None:
        return None, None, "USD"
    text = str(raw).strip()
    null_values = {"n/a", "not disclosed", "tbd", "-", "none", "null",
                   "", "not specified", "confidential", "negotiable"}
    if text.lower() in null_values:
        return None, None, "USD"

    currency = _detect_currency(text)

    # Remove currency symbols/codes for numeric parsing
    clean = re.sub(r"[$€£₹¥]", "", text)
    clean = re.sub(r"\b(usd|eur|gbp|egp|aed|sar|kwd|qar|inr|cad|aud)\b", "",
                   clean, flags=re.IGNORECASE)

    # Range: "1000 - 1500" or "1,000 – 1,500"
    range_match = re.split(r"\s*[-–—to]+\s*", clean.replace(",", ""))
    if len(range_match) >= 2:
        lo = _parse_salary_value(range_match[0])
        hi = _parse_salary_value(range_match[-1])
        if lo and hi and lo > hi:
            lo, hi = hi, lo
        return lo, hi, currency

    # Single value
    val = _parse_salary_value(clean.replace(",", ""))
    return val, None, currency


def clean_salaries(df: pd.DataFrame) -> pd.DataFrame:
    """Handle both salary_raw (combined) and pre-split SalaryMin/SalaryMax."""
    if "salary_raw" in df.columns:
        parsed = df["salary_raw"].apply(_split_salary_range)
        df["SalaryMin"] = parsed.apply(lambda x: x[0])
        df["SalaryMax"] = parsed.apply(lambda x: x[1])
        # Only set Currency if not already present
        if "Currency" not in df.columns:
            df["Currency"] = parsed.apply(lambda x: x[2])
        else:
            # Fill missing currencies from salary_raw
            missing_curr = df["Currency"].isna()
            df.loc[missing_curr, "Currency"] = parsed[missing_curr].apply(lambda x: x[2])
    else:
        for col in ["SalaryMin", "SalaryMax"]:
            if col not in df.columns:
                df[col] = None
            else:
                df[col] = pd.to_numeric(df[col], errors="coerce")

    return df


def normalize_currency(df: pd.DataFrame) -> pd.DataFrame:
    if "Currency" not in df.columns:
        df["Currency"] = "USD"
        return df

    def _clean(val):
        if pd.isna(val) or val is None:
            return "USD"
        c = str(val).strip().upper()
        sym = {"$": "USD", "€": "EUR", "£": "GBP", "₹": "INR"}
        if c in sym:
            return sym[c]
        return c if c in VALID_CURRENCIES else "USD"

    df["Currency"] = df["Currency"].apply(_clean)
    return df


# ─── Synthesise SourceUrl if missing ─────────────────────────────────────────

def fill_source_url(df: pd.DataFrame) -> pd.DataFrame:
    """
    If SourceUrl is missing, generate a deterministic synthetic URL
    from SourceSite + job_id_raw, so we can still deduplicate on it.
    """
    if "SourceUrl" not in df.columns:
        df["SourceUrl"] = None

    missing = df["SourceUrl"].isna() | (df["SourceUrl"].astype(str).str.strip() == "")

    site_col   = df.get("SourceSite",  pd.Series(["unknown"] * len(df), index=df.index))
    job_id_col = df.get("job_id_raw",  pd.Series(range(len(df)),        index=df.index))

    df.loc[missing, "SourceUrl"] = (
        "synthetic://" +
        site_col[missing].fillna("unknown").astype(str) + "/" +
        job_id_col[missing].fillna("").astype(str)
    )
    return df


# ─── STEP 8 & 9: Date Cleaning + IsActive ────────────────────────────────────

_RELATIVE_PATTERNS = re.compile(
    r"^\d+\s+(second|minute|hour|day|week|month|year)s?\s+ago$", re.IGNORECASE
)


def _parse_date(val, fallback=None) -> str | None:
    if pd.isna(val) or val is None:
        return fallback
    text = str(val).strip()
    # Reject relative dates like "1 hour ago" — use fallback
    if _RELATIVE_PATTERNS.match(text):
        return fallback
    try:
        return date_parser.parse(text, fuzzy=True).isoformat()
    except (ValueError, OverflowError, TypeError):
        return fallback


def clean_dates(df: pd.DataFrame) -> pd.DataFrame:
    scraped_now = datetime.now(timezone.utc).isoformat()

    # Best PostedAt source
    if "PostedAt" in df.columns:
        # Get scraped_at as fallback if available
        fallback_col = df.get("_scraped_at_raw", pd.Series([scraped_now] * len(df), index=df.index))
        df["PostedAt"] = [
            _parse_date(v, fallback=_parse_date(fb, fallback=scraped_now))
            for v, fb in zip(df["PostedAt"], fallback_col)
        ]
    else:
        df["PostedAt"] = scraped_now

    df["ScrapedAt"] = scraped_now
    df["IsActive"]  = True
    return df


# ─── STEP 10: Remove Invalid Rows ────────────────────────────────────────────

def remove_invalid_rows(df: pd.DataFrame):
    """Return (valid_df, rejected_df). Rejected rows saved to CSV."""
    from config import REJECTED_JOBS_CSV

    reject_records = []
    valid_mask = pd.Series([True] * len(df), index=df.index)

    for field in REQUIRED_FIELDS:
        if field not in df.columns:
            df[field] = None
        # Safely cast to string Series first, then check blank
        col_as_str = pd.Series(df[field].values, dtype=object).fillna("").astype(str).str.strip()
        missing = df[field].isna() | (col_as_str == "")
        newly_bad = missing & valid_mask
        for idx in df[newly_bad].index:
            reject_records.append({
                **df.loc[idx].to_dict(),
                "rejection_reason": f"Missing required field: {field}"
            })
        valid_mask &= ~missing

    valid_df    = df[valid_mask].copy()
    rejected_df = pd.DataFrame(reject_records)

    logger.info(f"Valid rows  : {len(valid_df):,}")
    logger.info(f"Rejected rows: {len(rejected_df):,}")

    if not rejected_df.empty:
        rejected_df.to_csv(REJECTED_JOBS_CSV, index=False, encoding="utf-8")
        logger.info(f"Rejected rows saved → {REJECTED_JOBS_CSV}")

    return valid_df, rejected_df


# ─── STEP 11: Deduplicate ─────────────────────────────────────────────────────

def deduplicate(df: pd.DataFrame, existing_urls: set, existing_keys: set) -> pd.DataFrame:
    initial = len(df)

    df = df[~df["SourceUrl"].isin(existing_urls)].copy()

    df["_dedup_key"] = (
        df["Title"].str.lower().str.strip().fillna("") + "||" +
        df["Company"].str.lower().str.strip().fillna("") + "||" +
        df["Location"].str.lower().str.strip().fillna("")
    )
    df = df[~df["_dedup_key"].isin(existing_keys)]
    df = df.drop_duplicates(subset=["SourceUrl"])
    df = df.drop_duplicates(subset=["_dedup_key"])
    df = df.drop(columns=["_dedup_key"])

    removed = initial - len(df)
    logger.info(f"Duplicates removed: {removed:,} | Remaining: {len(df):,}")
    return df.reset_index(drop=True)

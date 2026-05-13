"""
estimate_salaries.py — Smart salary estimation for jobs with hidden/missing salaries.

Uses role-based salary bands with seniority progression and intelligent variation.
Only estimates jobs where SalaryMin is null — never touches real salaries.
"""

import logging
import re
import hashlib
import pandas as pd

logger = logging.getLogger("jobalatica_etl")

# ─── Seniority detection ──────────────────────────────────────────────────────
SENIORITY_ORDER = ["Intern", "Entry", "Mid", "Senior", "Lead"]

SENIORITY_KEYWORDS = {
    "Intern":    ["intern", "trainee", "co-op", "coop"],
    "Entry":     ["junior", "jr", "jr.", "entry", "graduate", "fresher", "associate"],
    "Mid":       ["mid", "intermediate", "regular", "ii"],
    "Senior":    ["senior", "sr", "sr.", "experienced", "iii", "staff"],
    "Lead":      ["lead", "principal", "manager", "director", "head", "chief", "architect", "vp", "iv"],
}

ROLE_SPECIFIC_KEYWORDS = {
    "intern": "Intern",
    "trainee": "Intern",
    "junior": "Entry",
    "jr": "Entry",
    "senior": "Senior",
    "sr": "Senior",
    "lead": "Lead",
    "principal": "Lead",
    "head": "Lead",
    "chief": "Lead",
    "director": "Lead",
    "manager": "Lead",
    "architect": "Lead",
}

# ─── Role classification ──────────────────────────────────────────────────────
def classify_role(title: str) -> str:
    if not title:
        return "General"
    t = title.lower().strip()

    role_patterns = [
        (r"\b(data\s*(entry|clerk|typist)|admin(istrativ)?\s*(assistant|coordinator|support|clerk|specialist)?)\b", "Administrative"),
        (r"\b(account(ant|ing)?|audit(or)?|tax|treasury|payroll|finance|controller|cfo)\b", "Accounting"),
        (r"\b(sales\s*(rep|representative|associate|executive|engineer|consultant)?|account\s*manager|business\s*development|bde|bda)\b", "Sales"),
        (r"\b(marketing|brand|digital\s*m(arketing)?|social\s*media|seo|content|growth|pr|public\s*relations|communications|media)\b", "Marketing"),
        (r"\b(hr|human\s*resources|recruit(er|ing|ment)?|talent|people\s*(operations|partner)?|payroll|compensation|benefits)\b", "HR"),
        (r"\b(engineer(ing)?|developer|software|frontend|backend|full.?stack|web|mobile|app|ios|android|flutter|react|angular|vue|node|django|spring|api|devops|cloud|infrastructure|qa|quality\s*assurance|tester|test|automation|data\s*(scientist|analyst|engineer)?|machine\s*learning|ml|ai|sysadmin|it\s*support|network|security|cyber)\b", "Tech"),
        (r"\b(civil|architect(ure|ural)?|structural|mechanical|electrical|plumbing|mep|construction|site|survey(or|ing)?|draftsman|autocad|cad|bim|quantity\s*survey(or|ing)?)\b", "Engineering"),
        (r"\b(doctor|nurse|pharmac(ist|y)?|medical|clinical|healthcare|dentist|veterinary|lab\s*tech|radiolog(y|ist)?)\b", "Healthcare"),
        (r"\b(teacher|professor|instructor|lecturer|educator|academic|trainer|tutor|faculty)\b", "Education"),
        (r"\b(design(er)?|ui|ux|graphic|visual|creative|art\s*director|motion|animation|multimedia)\b", "Design"),
        (r"\b(lawyer|legal|attorney|paralegal|counsel|compliance|regulatory)\b", "Legal"),
        (r"\b(logistics|supply\s*chain|warehouse|inventory|procurement|purchasing|shipping|transport(ation)?|fleet)\b", "Logistics"),
        (r"\b(operation(s)?|project\s*manager|program\s*manager|product\s*(manager|owner)?|scrum\s*master|agile\s*coach|business\s*analyst)\b", "Management"),
        (r"\b(customer\s*(service|support|success|care)?|call\s*center|helpdesk|support\s*(engineer|specialist)?)\b", "Customer Support"),
        (r"\b(chef|cook|kitchen|barista|waiter|waitress|server|host|hospitality|hotel|restaurant|banquet|housekeeping|front\s*desk|concierge)\b", "Hospitality"),
        (r"\b(driver|delivery|courier|logistics\s*coordinator)\b", "Logistics"),
        (r"\b(security\s*(guard|officer)?|safety|hse|ehs|environmental)\b", "Security"),
    ]

    for pattern, category in role_patterns:
        if re.search(pattern, t):
            return category

    # Check title for seniority keywords as last resort classification
    # e.g. "Senior Specialist" → "General"
    return "General"

# ─── Salary bands (EGP monthly) ────────────────────────────────────────────────
SALARY_BANDS = {
    "Administrative":    {"Intern": (2000, 4000), "Entry": (3000, 6000),  "Mid": (6000, 10000),  "Senior": (10000, 15000), "Lead": (15000, 22000)},
    "Accounting":        {"Intern": (3000, 5000), "Entry": (5000, 10000), "Mid": (10000, 18000), "Senior": (18000, 30000), "Lead": (28000, 50000)},
    "Sales":             {"Intern": (2500, 4500), "Entry": (4000, 8000),  "Mid": (8000, 15000),  "Senior": (15000, 28000), "Lead": (25000, 45000)},
    "Marketing":         {"Intern": (2500, 4500), "Entry": (4000, 8000),  "Mid": (8000, 15000),  "Senior": (15000, 25000), "Lead": (25000, 40000)},
    "HR":                {"Intern": (2500, 4000), "Entry": (4000, 7000),  "Mid": (7000, 14000),  "Senior": (14000, 22000), "Lead": (22000, 35000)},
    "Tech":              {"Intern": (3000, 6000), "Entry": (6000, 12000), "Mid": (12000, 22000), "Senior": (22000, 40000), "Lead": (40000, 70000)},
    "Engineering":       {"Intern": (3000, 5000), "Entry": (5000, 10000), "Mid": (10000, 18000), "Senior": (18000, 30000), "Lead": (30000, 50000)},
    "Healthcare":        {"Intern": (2500, 4500), "Entry": (4000, 8000),  "Mid": (8000, 15000),  "Senior": (15000, 28000), "Lead": (28000, 45000)},
    "Education":         {"Intern": (2000, 3500), "Entry": (3500, 6000),  "Mid": (6000, 10000),  "Senior": (10000, 18000), "Lead": (18000, 30000)},
    "Design":            {"Intern": (2500, 4500), "Entry": (4000, 8000),  "Mid": (8000, 14000),  "Senior": (14000, 24000), "Lead": (24000, 40000)},
    "Legal":             {"Intern": (3000, 5000), "Entry": (5000, 10000), "Mid": (10000, 18000), "Senior": (18000, 35000), "Lead": (35000, 60000)},
    "Logistics":         {"Intern": (2500, 4000), "Entry": (4000, 7000),  "Mid": (7000, 12000),  "Senior": (12000, 20000), "Lead": (20000, 32000)},
    "Management":        {"Intern": (3500, 6000), "Entry": (6000, 10000), "Mid": (10000, 18000), "Senior": (18000, 30000), "Lead": (30000, 55000)},
    "Customer Support":  {"Intern": (2000, 3500), "Entry": (3500, 6000),  "Mid": (6000, 10000),  "Senior": (10000, 16000), "Lead": (16000, 25000)},
    "Hospitality":       {"Intern": (1500, 3000), "Entry": (2500, 5000),  "Mid": (5000, 9000),   "Senior": (9000, 15000),  "Lead": (15000, 25000)},
    "Security":          {"Intern": (2000, 3500), "Entry": (3000, 5500),  "Mid": (5500, 10000),  "Senior": (10000, 16000), "Lead": (16000, 25000)},
    "General":           {"Intern": (2000, 3500), "Entry": (3500, 6000),  "Mid": (6000, 11000),  "Senior": (11000, 18000), "Lead": (18000, 30000)},
}


def _detect_seniority(title: str, experience_level: str) -> str:
    """Detect seniority from title text and ExperienceLevel field."""
    if not title:
        return experience_level if experience_level in SENIORITY_ORDER else "Mid"

    t = title.lower().strip()

    # Check ExperienceLevel field (normalized value)
    if experience_level in SENIORITY_ORDER:
        # Still scan title for more specific keywords
        for level, keywords in SENIORITY_KEYWORDS.items():
            for kw in keywords:
                if re.search(rf"\b{re.escape(kw)}\b", t):
                    return level
        return experience_level

    # Scan title for seniority keywords
    for level, keywords in SENIORITY_KEYWORDS.items():
        for kw in keywords:
            if re.search(rf"\b{re.escape(kw)}\b", t):
                return level

    # Default: return "Mid" for most roles
    return "Mid"


def _determined_variation(title: str, salary_range: tuple, job_index: int = 0) -> tuple:
    """
    Add controlled, deterministic variation based on job title hash and per-job index.
    Same title gets different estimates per job — no cloned values.
    """
    lo, hi = salary_range
    spread = hi - lo

    # Combine title hash with job_index for per-instance variation
    title_seed = int(hashlib.md5(title.encode()).hexdigest()[:8], 16)
    combined_seed = (title_seed * 1000 + job_index * 7) % 10000
    # Variation factor: -20% to +20% of spread
    variation = ((combined_seed % 100) - 50) / 100.0 * spread * 0.20
    mid = (lo + hi) / 2
    adj_mid = mid + variation

    # Ensure minimum range width with per-job stretch factor
    stretch = 0.6 + (combined_seed % 40) / 100.0
    range_width = max(spread * stretch, spread * 0.35 + abs(variation) * 0.25)
    new_lo = max(adj_mid - range_width / 2, lo * 0.8)
    new_hi = min(adj_mid + range_width / 2, hi * 1.2)

    return (round(new_lo, -2), round(new_hi, -2))


def _estimate_for_job(title: str, experience_level: str, job_index: int = 0) -> tuple:
    """
    Determine (IsSalaryEstimated, EstimatedSalaryMin, EstimatedSalaryMax)
    for a job with a hidden/missing salary.
    """
    category = classify_role(title)
    seniority = _detect_seniority(title, experience_level)

    bands = SALARY_BANDS.get(category, SALARY_BANDS["General"])
    band = bands.get(seniority, bands["Mid"])

    # Apply intelligent variation with per-job index
    varied = _determined_variation(title, band, job_index)

    return (True, varied[0], varied[1])


def calculate_estimates(df: pd.DataFrame, existing_df: pd.DataFrame = None) -> pd.DataFrame:
    """
    Estimate salaries for jobs with hidden/missing salary data.
    Does NOT modify real salaries — only fills EstimatedSalary fields.
    """
    df = df.copy()

    # Also incorporate existing data for better overall awareness (future use)
    # Currently uses purely rule-based estimation for consistency

    def _apply(row):
        if pd.notna(row.get("SalaryMin")):
            return (False, None, None)
        idx = row.name if isinstance(row.name, (int, float)) else 0
        return _estimate_for_job(
            str(row.get("Title", "") or ""),
            str(row.get("ExperienceLevel", "") or ""),
            int(idx)
        )

    estimates = df.apply(_apply, axis=1)
    df["IsSalaryEstimated"] = [e[0] for e in estimates]
    df["EstimatedSalaryMin"] = [e[1] for e in estimates]
    df["EstimatedSalaryMax"] = [e[2] for e in estimates]

    est_count = df["IsSalaryEstimated"].sum()
    logger.info(f"Calculated salary estimates for {est_count:,} jobs.")
    return df

# Jobalatica ETL Pipeline Implementation Guide

## Objective

Build a complete ETL pipeline for the Jobalatica project that:

1. Reads all scraped JSON job files
2. Cleans and normalizes the data according to the SQLite schema
3. Removes duplicates
4. Normalizes skills into canonical skills
5. Generates `JobSkills` relationships
6. Validates all data before insertion
7. Appends the cleaned data into the existing SQLite database
8. Saves rejected/invalid rows for debugging
9. Runs automatically from one executable file:

```bash
python run_pipeline.py
```

---

# Database Information

## Database Engine

SQLite

## Database File

```text
/database/JobPulse.db
```

---

# Important Rules

## DO NOT TOUCH THESE TABLES

The ETL pipeline must NEVER modify:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- Any AspNet* Identity table

These belong to the ASP.NET Core authentication system.

---

# The Pipeline ONLY Manages

- Jobs
- Skills
- JobSkills
- DemandSnapshots

---

# Required Project Structure

```text
project/
│
├── seeddata/
│   ├── raw/
│   │   ├── jobs1.json
│   │   ├── jobs2.json
│   │   ├── jobs3.json
│   │   ├── jobs4.json
│   │   └── jobs5.json
│   │
│   ├── cleaned/
│   │   ├── clean_jobs.csv
│   │   ├── clean_jobskills.csv
│   │   ├── rejected_jobs.csv
│   │   └── validation_report.txt
│
├── database/
│   └── JobPulse.db
│
├── pipeline/
│   ├── config.py
│   ├── load_json.py
│   ├── clean_jobs.py
│   ├── normalize_skills.py
│   ├── generate_jobskills.py
│   ├── validate.py
│   ├── load_sqlite.py
│   ├── snapshots.py
│   └── utils.py
│
├── logs/
│   └── pipeline.log
│
├── requirements.txt
│
└── run_pipeline.py
```

---

# Full Pipeline Flow

```text
JSON FILES
    ↓
LOAD RAW DATA
    ↓
MERGE ALL FILES
    ↓
STANDARDIZE COLUMNS
    ↓
CLEAN TEXT
    ↓
NORMALIZE EXPERIENCE LEVELS
    ↓
CLEAN SALARIES
    ↓
CLEAN DATES
    ↓
REMOVE INVALID ROWS
    ↓
REMOVE DUPLICATES
    ↓
NORMALIZE SKILLS
    ↓
GENERATE JOBSKILLS
    ↓
VALIDATE FK RELATIONSHIPS
    ↓
APPEND INTO SQLITE
    ↓
SAVE REPORTS + LOGS
```

---

# STEP 1 — Load Existing Database Data

Before inserting anything, the pipeline must read existing database data.

This is required to:

- avoid duplicate jobs
- reuse existing skills
- preserve mock data
- prevent foreign key conflicts

## Read Existing Tables

Read:

- Jobs
- Skills
- JobSkills

using sqlite3 + pandas.

---

# STEP 2 — Load All JSON Files

The pipeline must:

1. Read every `.json` file inside:

```text
/data/raw/
```

2. Merge all jobs into one dataframe.

3. Support:

- list JSON structure
- single-object JSON structure

---

# STEP 3 — Standardize Column Names

The pipeline must rename raw scraped columns into schema-compliant column names.

## Required Jobs Schema Columns

```text
Title
Company
Location
Country
SalaryMin
SalaryMax
Currency
ExperienceLevel
SourceUrl
SourceSite
PostedAt
ScrapedAt
IsActive
```

## Example Mapping

```python
COLUMN_MAP = {
    "job_title": "Title",
    "company_name": "Company",
    "city": "Location",
    "country_name": "Country",
    "url": "SourceUrl",
    "site": "SourceSite"
}
```

The implementation should support easily adding new mappings.

---

# STEP 4 — Clean Text Fields

The pipeline must clean:

- Title
- Company
- Location
- Country

Cleaning Rules:

- trim spaces
- collapse repeated spaces
- remove hidden characters
- convert invalid values to NULL

Example:

```text
"  Senior   Python Developer  "
```

becomes:

```text
"Senior Python Developer"
```

---

# STEP 5 — Normalize Experience Levels

The database only allows these values:

```text
Entry
Mid
Senior
Lead
```

All scraped variations must map to one of these.

## Example Mapping

```python
LEVEL_MAP = {
    "junior": "Entry",
    "jr": "Entry",
    "entry-level": "Entry",

    "mid": "Mid",
    "mid-level": "Mid",

    "senior": "Senior",
    "sr": "Senior",

    "lead": "Lead",
    "principal": "Lead"
}
```

Rows with unmapped experience levels should be rejected.

---

# STEP 6 — Salary Cleaning

Salary fields:

- SalaryMin
- SalaryMax

must:

- become numeric
- support decimal values
- remove currency symbols
- remove commas
- handle missing salaries safely

## Examples

| Raw | Clean |
|---|---|
| "$3,000" | 3000 |
| "5k" | 5000 |
| "Not disclosed" | NULL |

---

# STEP 7 — Currency Normalization

Currencies must be standardized.

Examples:

| Raw | Clean |
|---|---|
| usd | USD |
| egp | EGP |
| eur | EUR |

Invalid currencies should be rejected.

---

# STEP 8 — Date Cleaning

The pipeline must clean:

- PostedAt

Rules:

- convert to datetime
- invalid dates become NULL
- rows with invalid PostedAt must be rejected

The pipeline must also automatically generate:

```text
ScrapedAt = current UTC datetime
```

---

# STEP 9 — IsActive Column

Every newly scraped job should default to:

```text
IsActive = True
```

---

# STEP 10 — Remove Invalid Rows

Rows missing required fields must be rejected.

## Required Fields

```text
Title
Company
Location
Country
Currency
ExperienceLevel
SourceUrl
SourceSite
PostedAt
```

Rejected rows must be saved into:

```text
/data/cleaned/rejected_jobs.csv
```

with an additional column:

```text
rejection_reason
```

---

# STEP 11 — Deduplicate Jobs

The pipeline must remove duplicates against:

1. existing database jobs
2. duplicate rows inside scraped data itself

## Deduplication Key

Use:

```text
SourceUrl
```

as the primary deduplication key.

Secondary key:

```text
Title + Company + Location
```

---

# STEP 12 — Skills Normalization

This is the MOST IMPORTANT PART of the project.

The pipeline must normalize all scraped skills into canonical skill names.

## Canonical Skills

```text
JavaScript
TypeScript
Python
Java
C#
.NET
React
Angular
Vue
Node.js
SQL
PostgreSQL
MongoDB
Docker
AWS
Azure
Git
HTML/CSS
PHP
Kotlin
Swift
Go
Rust
Flutter
GraphQL
```

---

# Skill Mapping Rules

Examples:

| Raw Skill | Canonical Skill |
|---|---|
| python3 | Python |
| py | Python |
| js | JavaScript |
| postgres | PostgreSQL |
| dotnet | .NET |

---

# Existing Skills Table Must Be Reused

The pipeline MUST NOT recreate the Skills table every run.

Instead:

1. Read existing Skills table
2. Build skill-name → skill-id mapping
3. Reuse existing IDs
4. Insert only new canonical skills if needed

---

# STEP 13 — Generate JobSkills

After jobs are inserted:

1. Extract skills from job descriptions/tags
2. Normalize skills
3. Match skill IDs
4. Generate JobSkills rows

## Requirements

- no duplicate JobSkills rows
- JobId must exist
- SkillId must exist

---

# STEP 14 — Foreign Key Validation

Before insertion:

Validate:

- JobSkills.JobId exists in Jobs
- JobSkills.SkillId exists in Skills

Rows failing FK validation must be rejected.

---

# STEP 15 — Generate DemandSnapshots

The pipeline should generate aggregated trend data.

## Group By

- normalized job title
- location
- week/date

## Compute

- PostingCount
- AvgSalaryMin
- AvgSalaryMax

Store results in:

```text
DemandSnapshots
```

---

# STEP 16 — Save Cleaned Files

The pipeline must export:

```text
/data/cleaned/clean_jobs.csv
/data/cleaned/clean_jobskills.csv
/data/cleaned/rejected_jobs.csv
/data/cleaned/validation_report.txt
```

---

# STEP 17 — Insert Into SQLite

Use:

```python
if_exists="append"
```

NEVER:

```python
if_exists="replace"
```

because the database already contains mock data.

The pipeline must preserve existing data.

---

# STEP 18 — Logging

Create:

```text
/logs/pipeline.log
```

Log:

- total rows loaded
- cleaned rows
- rejected rows
- inserted rows
- duplicate count
- execution time
- insertion errors

---

# STEP 19 — Main Executable File

Create:

```text
run_pipeline.py
```

Running:

```bash
python run_pipeline.py
```

must execute the full ETL flow automatically.

---

# Required Python Libraries

```text
pandas
sqlite3
numpy
python-dateutil
sqlalchemy
```

Optional:

```text
great_expectations
loguru
```

---

# Recommended Implementation Order

## Version 1

Implement:

```text
JSON → Clean Jobs → Insert Into SQLite
```

---

## Version 2

Add:

- skills normalization
- JobSkills generation
- validation layer

---

## Version 3

Add:

- DemandSnapshots
- logging
- reporting
- automation

---

# Important Engineering Rules

## Rule 1

Never insert raw scraped data directly into the database.

---

## Rule 2

Always validate before insertion.

---

## Rule 3

Always preserve mock data.

---

## Rule 4

Never modify authentication tables.

---

## Rule 5

Skills normalization is the highest-priority feature.

---

# Final Goal

The final system should behave like this:

```bash
python run_pipeline.py
```

and automatically:

```text
1. Read all JSON files
2. Clean data
3. Normalize data
4. Validate rows
5. Remove duplicates
6. Generate relationships
7. Append into SQLite
8. Save reports/logs
```

with no manual intervention.


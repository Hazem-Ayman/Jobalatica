"""
config.py — Central configuration for the Jobalatica ETL Pipeline
"""

import os

# ─── Base Paths ──────────────────────────────────────────────────────────────
BASE_DIR    = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RAW_DIR     = os.path.join(BASE_DIR, "seeddata", "raw")
CLEANED_DIR = os.path.join(BASE_DIR, "seeddata", "cleaned")
DB_PATH     = os.path.join(BASE_DIR, "JobPulse.db")
LOG_DIR     = os.path.join(BASE_DIR, "logs")
LOG_FILE    = os.path.join(LOG_DIR, "pipeline.log")

# ─── Output Files ─────────────────────────────────────────────────────────────
CLEAN_JOBS_CSV      = os.path.join(CLEANED_DIR, "clean_jobs.csv")
CLEAN_JOBSKILLS_CSV = os.path.join(CLEANED_DIR, "clean_jobskills.csv")
REJECTED_JOBS_CSV   = os.path.join(CLEANED_DIR, "rejected_jobs.csv")
VALIDATION_REPORT   = os.path.join(CLEANED_DIR, "validation_report.txt")

# ─── Column Mapping (raw scraped names → schema names) ────────────────────────
COLUMN_MAP = {
    # Title variants
    "job_title":         "Title",
    "title":             "Title",
    "position":          "Title",
    "role":              "Title",
    "job_name":          "Title",

    # Company variants
    "company_name":      "Company",
    "company":           "Company",
    "employer":          "Company",
    "organization":      "Company",

    # Location variants
    "city":              "Location",
    "location":          "Location",
    "job_location":      "Location",
    "work_location":     "Location",

    # Country variants — may not exist in raw data; extracted from location
    "country_name":      "Country",
    "country":           "Country",

    # Salary — combined range field in raw data (e.g. "$1,000 - $1,500")
    "salary":            "salary_raw",
    "salary_min":        "SalaryMin",
    "min_salary":        "SalaryMin",
    "salary_from":       "SalaryMin",
    "salary_max":        "SalaryMax",
    "max_salary":        "SalaryMax",
    "salary_to":         "SalaryMax",

    # Currency — extracted from salary_raw string
    "currency":          "Currency",
    "currency_code":     "Currency",

    # Experience Level
    "experience_level":  "ExperienceLevel",
    "experience":        "experience_raw",   # raw string e.g. "Management·5-15 Years"
    "seniority":         "ExperienceLevel",
    "seniority_level":   "ExperienceLevel",
    "level":             "ExperienceLevel",
    "job_level":         "ExperienceLevel",

    # Source URL
    "url":               "SourceUrl",
    "source_url":        "SourceUrl",
    "job_url":           "SourceUrl",
    "link":              "SourceUrl",

    # Source Site
    "site":              "SourceSite",
    "source":            "SourceSite",
    "source_site":       "SourceSite",
    "platform":          "SourceSite",
    "board":             "SourceSite",
    "job_board":         "SourceSite",

    # Dates
    "posted_at":         "PostedAt",
    "date_posted":       "PostedAt",
    "post_date":         "PostedAt",
    "posted_date":       "PostedAt",
    "date":              "PostedAt",
    "published_at":      "PostedAt",
    # Note: scraped_at is handled explicitly in clean_jobs.py (not here)

    # Internal ID (used to synthesise SourceUrl if missing)
    "job_id":            "job_id_raw",

    # Skills / Description
    "skills":            "skills_raw",
    "required_skills":   "skills_raw",
    "tech_stack":        "skills_raw",
    "tags":              "skills_raw",
    "technologies":      "skills_raw",
    "description":       "description_raw",
    "job_description":   "description_raw",

    # Industry (used for skill hints)
    "industry":          "industry_raw",
}

# ─── Experience Level Normalization ──────────────────────────────────────────
LEVEL_MAP = {
    # Entry
    "entry":             "Entry",
    "entry-level":       "Entry",
    "entry level":       "Entry",
    "junior":            "Entry",
    "jr":                "Entry",
    "jr.":               "Entry",
    "graduate":          "Entry",
    "intern":            "Entry",
    "internship":        "Entry",
    "associate":         "Entry",
    "fresher":           "Entry",

    # Mid
    "mid":               "Mid",
    "mid-level":         "Mid",
    "mid level":         "Mid",
    "intermediate":      "Mid",
    "regular":           "Mid",
    "ii":                "Mid",

    # Senior
    "senior":            "Senior",
    "sr":                "Senior",
    "sr.":               "Senior",
    "experienced":       "Senior",
    "iii":               "Senior",

    # Lead
    "lead":              "Lead",
    "principal":         "Lead",
    "staff":             "Lead",
    "manager":           "Lead",
    "director":          "Lead",
    "head":              "Lead",
    "architect":         "Lead",
    "iv":                "Lead",
}

# ─── Canonical Skills ─────────────────────────────────────────────────────────
CANONICAL_SKILLS = [
    "JavaScript", "TypeScript", "Python", "Java", "C#", ".NET",
    "React", "Angular", "Vue", "Node.js", "SQL", "PostgreSQL",
    "MongoDB", "Docker", "AWS", "Azure", "Git", "HTML/CSS",
    "PHP", "Kotlin", "Swift", "Go", "Rust", "Flutter", "GraphQL",
    "Redis", "Kubernetes", "Linux", "Django", "FastAPI", "Spring",
    "MySQL", "Firebase", "Terraform", "Jenkins", "CI/CD",
    "Machine Learning", "Deep Learning", "TensorFlow", "PyTorch",
    "Pandas", "NumPy", "Spark", "Scala", "Ruby", "Rails",
    "C++", "C", "Bash", "PowerShell", "REST API", "Microservices",
]

# ─── Skill Alias → Canonical Mapping ─────────────────────────────────────────
SKILL_ALIAS_MAP = {
    # Python
    "python3": "Python", "py": "Python", "python 3": "Python",

    # JavaScript
    "js": "JavaScript", "javascript": "JavaScript", "es6": "JavaScript",
    "es2015": "JavaScript", "ecmascript": "JavaScript",

    # TypeScript
    "ts": "TypeScript", "typescript": "TypeScript",

    # Java
    "java": "Java", "java8": "Java", "java 8": "Java", "java11": "Java",

    # C#
    "c#": "C#", "csharp": "C#", "c sharp": "C#",

    # .NET
    "dotnet": ".NET", ".net": ".NET", "asp.net": ".NET",
    "asp net": ".NET", "aspnet": ".NET", ".net core": ".NET",
    "netcore": ".NET",

    # React
    "react": "React", "reactjs": "React", "react.js": "React",

    # Angular
    "angular": "Angular", "angularjs": "Angular", "angular2": "Angular",

    # Vue
    "vue": "Vue", "vuejs": "Vue", "vue.js": "Vue",

    # Node.js
    "node": "Node.js", "nodejs": "Node.js", "node.js": "Node.js",

    # SQL
    "sql": "SQL", "t-sql": "SQL", "tsql": "SQL", "pl/sql": "SQL",

    # PostgreSQL
    "postgres": "PostgreSQL", "postgresql": "PostgreSQL", "pgsql": "PostgreSQL",

    # MongoDB
    "mongo": "MongoDB", "mongodb": "MongoDB",

    # Docker
    "docker": "Docker",

    # AWS
    "aws": "AWS", "amazon web services": "AWS",

    # Azure
    "azure": "Azure", "microsoft azure": "Azure",

    # Git
    "git": "Git", "github": "Git", "gitlab": "Git",

    # HTML/CSS
    "html": "HTML/CSS", "css": "HTML/CSS", "html5": "HTML/CSS",
    "css3": "HTML/CSS", "html/css": "HTML/CSS", "scss": "HTML/CSS",
    "sass": "HTML/CSS",

    # PHP
    "php": "PHP",

    # Kotlin
    "kotlin": "Kotlin",

    # Swift
    "swift": "Swift",

    # Go
    "go": "Go", "golang": "Go",

    # Rust
    "rust": "Rust",

    # Flutter
    "flutter": "Flutter", "dart": "Flutter",

    # GraphQL
    "graphql": "GraphQL", "gql": "GraphQL",

    # Redis
    "redis": "Redis",

    # Kubernetes
    "kubernetes": "Kubernetes", "k8s": "Kubernetes",

    # Linux
    "linux": "Linux", "unix": "Linux",

    # Django
    "django": "Django",

    # FastAPI
    "fastapi": "FastAPI",

    # Spring
    "spring": "Spring", "spring boot": "Spring", "springboot": "Spring",

    # MySQL
    "mysql": "MySQL", "mariadb": "MySQL",

    # Firebase
    "firebase": "Firebase",

    # Terraform
    "terraform": "Terraform",

    # Jenkins
    "jenkins": "Jenkins",

    # CI/CD
    "ci/cd": "CI/CD", "cicd": "CI/CD", "ci cd": "CI/CD",
    "github actions": "CI/CD",

    # Machine Learning
    "machine learning": "Machine Learning", "ml": "Machine Learning",

    # Deep Learning
    "deep learning": "Deep Learning", "dl": "Deep Learning",

    # TensorFlow
    "tensorflow": "TensorFlow", "tf": "TensorFlow",

    # PyTorch
    "pytorch": "PyTorch", "torch": "PyTorch",

    # Pandas
    "pandas": "Pandas",

    # NumPy
    "numpy": "NumPy", "np": "NumPy",

    # Spark
    "spark": "Spark", "apache spark": "Spark", "pyspark": "Spark",

    # Scala
    "scala": "Scala",

    # Ruby
    "ruby": "Ruby",

    # Rails
    "rails": "Rails", "ruby on rails": "Rails", "ror": "Rails",

    # C++
    "c++": "C++", "cpp": "C++",

    # C
    "c": "C",

    # Bash
    "bash": "Bash", "shell": "Bash", "shell scripting": "Bash",

    # PowerShell
    "powershell": "PowerShell", "ps": "PowerShell",

    # REST API
    "rest": "REST API", "rest api": "REST API", "restful": "REST API",
    "api": "REST API",

    # Microservices
    "microservices": "Microservices", "micro services": "Microservices",
    "microservice": "Microservices",
}

# ─── Skill → Category Mapping ─────────────────────────────────────────────────
SKILL_CATEGORY_MAP = {
    "Python": "Language", "JavaScript": "Language", "TypeScript": "Language",
    "Java": "Language", "C#": "Language", "PHP": "Language", "Go": "Language",
    "Rust": "Language", "Kotlin": "Language", "Swift": "Language",
    "Scala": "Language", "Ruby": "Language", "C++": "Language", "C": "Language",
    "Bash": "Language", "PowerShell": "Language",

    "React": "Framework", "Angular": "Framework", "Vue": "Framework",
    "Node.js": "Runtime", ".NET": "Framework", "Django": "Framework",
    "FastAPI": "Framework", "Spring": "Framework", "Rails": "Framework",
    "Flutter": "Framework",

    "SQL": "Database", "PostgreSQL": "Database", "MongoDB": "Database",
    "MySQL": "Database", "Redis": "Database", "Firebase": "Database",

    "Docker": "DevOps", "Kubernetes": "DevOps", "AWS": "Cloud",
    "Azure": "Cloud", "Terraform": "DevOps", "Jenkins": "DevOps",
    "CI/CD": "DevOps", "Linux": "OS",

    "Git": "Tool", "GraphQL": "API", "REST API": "API",
    "Microservices": "Architecture",

    "Machine Learning": "AI/ML", "Deep Learning": "AI/ML",
    "TensorFlow": "AI/ML", "PyTorch": "AI/ML",
    "Pandas": "Data", "NumPy": "Data", "Spark": "Data",

    "HTML/CSS": "Frontend",
}

# ─── Required Fields for a Valid Row ─────────────────────────────────────────
REQUIRED_FIELDS = ["Title", "Company", "Location", "Country",
                   "Currency", "ExperienceLevel", "SourceUrl",
                   "SourceSite", "PostedAt"]

# ─── Valid Currencies ─────────────────────────────────────────────────────────
VALID_CURRENCIES = {"USD", "EUR", "GBP", "EGP", "AED", "SAR", "CAD",
                    "AUD", "INR", "JPY", "CNY", "CHF", "TRY", "BRL",
                    "MXN", "SGD", "KWD", "QAR"}

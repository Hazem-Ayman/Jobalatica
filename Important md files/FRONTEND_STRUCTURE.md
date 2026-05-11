# JobPulse Frontend Structure & Placement Reference

This document maps the structural layout, components, and placeholders for the JobPulse application. It is intended for AI reference to ensure functional parity and consistent placement when generating new frontend code.

---

## 1. Global Components

### 1.1 Navigation Bar (`_Navbar.cshtml`)
- **Logo Area (Left):** Link to Home with text "JobPulse".
- **Primary Nav (Center):** Links: "Home", "Jobs", "Rankings".
- **Action Area (Right):**
  - **Authenticated:** 
    - User Menu Button (Circle with initial).
    - Dropdown Menu: "Profile", "Saved Jobs", "Sign out" (POST form).
  - **Unauthenticated:**
    - "Log in" link.
    - "Get started" button (primary action).
- **Mobile:** Hamburger toggle for main menu.

---

## 2. Page Layouts

### 2.1 Home Page (`Home/Index.cshtml`)
- **Hero Section:**
  - Header (H1) + Subheader (P).
  - Search Form: Text Input ("Job title or skill...") + "Analyze Market" Button.
- **Stats Grid (4 Columns):**
  - Live Jobs Count, Companies Count, Skills Tracked Count, Salary Points Count.
- **Main Content (Two Columns):**
  - **Column 1 (2/3):** "Trending Roles" Chart (Canvas).
  - **Column 2 (1/3):** "High-Demand Skills" Cloud (Tag links).
- **Secondary Content:**
  - "Recent Opportunities" Section: List of Job Cards + "View all jobs" link.
- **Personalized/Teaser Section (Full Width):**
  - Authenticated: List of Job Cards based on profile.
  - Unauthenticated: "Create Free Account" and "Sign In" buttons + Blurred background cards.
- **Salary Modal (Conditional):**
  - Popup with icon, header, description, "Contribute Now" button, and "Maybe later" button.

### 2.2 Job Marketplace (`Jobs/Index.cshtml`)
- **Layout:** Two-column Sidebar Layout.
- **Sidebar (Left, 1/4):**
  - Filter Form:
    - Keywords Input (Text).
    - Location Select (Dropdown).
    - Experience Radio Group (Pill/Button style: Entry, Mid, Senior, Lead, Any).
    - Min Salary Input (Number).
    - "Clear All Filters" link.
- **Results Area (Right, 3/4):**
  - Results Header: Count text ("X Results Found") + "Sort by" Select (Dropdown).
  - Results List: Vertical stack of Job Cards.
  - Pagination: "Previous" button + Page info + "Next" button.

### 2.3 Rankings Page (`Rankings/Index.cshtml`)
- **Header:** Title + Analysis period description.
- **Roles Analysis Section:**
  - Toggle Switch: Monthly / Quarterly.
  - "Top Roles" Chart (Horizontal Bar Chart Canvas).
  - Roles Table: Rank, Role Title, Postings Count, Trend %, "Details" link.
- **Industry Skills Section:**
  - Grid of Skill Cards: Category label, Name, Postings Count, Demand level badge.

### 2.4 Profile / Settings Page (`Profile/Index.cshtml`)
- **Layout:** Two-column Sidebar Layout.
- **Sidebar (Left):**
  - Identity Node: Large circle initial, Display Name, Email.
  - Quick Stats: Saved count, Skills count.
  - Local Nav: "Settings" (Active), "Saved Jobs".
- **Form Area (Right):**
  - Header: "Account Settings".
  - Field Group 1 (2 columns): "Display Name" (Input), "Experience Level" (Select).
  - Skill Matrix Section: Grid of "Skill Pills" (Checkboxes styled as buttons).
  - Footer Action: "Save Profile Changes" button.

### 2.5 Salary Contribution Page (`Salary/Submit.cshtml`)
- **Header:** Title + description about anonymity.
- **Form Card:**
  - Field Group 1: "Exact Job Title" (Input), "Location" (Input).
  - Field Group 2: "Annual Gross Salary" (Input with currency prefix) + "Currency" (Select).
  - Experience Slider: Header + Label + Range Input + min/max markers.
  - Skills Area: "Primary Skills" (Textarea).
  - Footer: Privacy note icon + "Submit Market Data" button.

---

## 3. Repeated Elements

### 3.1 Job Card (`_JobCard.cshtml`)
- **Top Row:** Title (Link) + Company Name (Left), Save Button (Heart Icon) (Right).
- **Middle Row:** Location Icon + Text, Salary Icon + Text.
- **Tags Row:** List of Skill Tags (Limit 4) + Overflow count badge.
- **Bottom Row:** Metadata (Posted Date via Source) + "View Details" link.

---

## 4. Navigation & Routing Path Logic
- **Jobs Search:** `GET /Jobs?query={q}&location={l}&experienceLevel={e}&salaryMin={s}`
- **Unsave/Delete Job:** `POST /Jobs/Unsave?jobId={id}`
- **Save Profile:** `POST /Profile/SaveProfile`
- **Salary Submit:** `POST /Salary/Submit`
- **Role Details:** `GET /Rankings/RoleDetail?title={title}`

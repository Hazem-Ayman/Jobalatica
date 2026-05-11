# UI Design Specification — JobPulse

This document lists all the interactive UI elements, forms, and placeholders currently in the JobPulse project. Use this to redesign the frontend while keeping the same logic and identifiers.

---

## 1. Global / Layout Components
- **Navbar (`_Navbar.cshtml`)**
  - Brand Link: `Home/Index`
  - Navigation Links: `Jobs/Index`, `Rankings/Index`
  - Auth Section (Logged Out): "Log in" link, "Get started" button.
  - Auth Section (Logged In): User Initials Circle (e.g., "JD"), Dropdown with "Profile", "Saved Jobs", "Sign out".
  
- **Footer (None currently)**
  - Redesign should include a simple footer with project links.

---

## 2. Landing Page (`Home/Index.cshtml`)
- **Hero Section**
  - Headline Placeholder: "Stop guessing. Start winning."
  - Subheadline Placeholder: "Real-time market data on salaries..."
  - Search Input: `name="query"`, Placeholder: "Job title or skill..."
  - Analyze Button: `type="submit"`
- **Stat Counters**
  - 4 Cards: Each has a `data-count-to` attribute (Live Jobs, Companies, Skills, Salary Points).
- **Charts**
  - Canvas ID: `trendingRolesChart` (Horizontal Bar Chart).
- **Skill Cloud**
  - Loop of links: `/Rankings/RoleDetail?title={Name}`.
- **Job Feed**
  - Grid of `_JobCard` components.
- **Personalized Section**
  - Conditional: Blurred placeholder grid with "Create Free Account" CTA if logged out.
- **Salary Modal**
  - ID: `salary-modal`
  - Button: "Contribute Now" -> `/Salary/Submit`.

---

## 3. Job Search Page (`Jobs/Index.cshtml`)
- **Sidebar Filters**
  - Input: `name="query"` (Live search trigger).
  - Select: `name="location"` (Cairo, Dubai, Riyadh, Remote).
  - Radio Group: `name="experienceLevel"` (Entry, Mid, Senior, Lead).
  - Input: `name="salaryMin"` (Number).
  - Button: "Clear All Filters" -> `/Jobs`.
- **Results Container**
  - ID: `job-results` (Target for HTMX updates).
  - Sort Select: "Newest First", "Highest Salary".
  - Pagination Buttons: "Previous", "Next".

---

## 4. Job Detail Page (`Jobs/Detail.cshtml`)
- **Header Card**
  - Title, Company, Location, Date.
  - Button: "Visit Original Posting" (External Link).
  - Button ID: `save-btn` (Toggles "Save" / "Saved").
  - Progress Bar: "Market Demand Score".
- **Main Content**
  - "About This Role" Section.
  - Required Skills List (Links to Rankings).
- **Salary Intelligence**
  - Visual Range Bar: Market Average vs Posted Range.
- **Sidebar**
  - Canvas ID: `skillDemandChart` (Radar Chart).
  - "Similar Jobs" vertical list.

---

## 5. Market Rankings Page (`Rankings/Index.cshtml`)
- **Charts**
  - Canvas ID: `topRolesChart` (Horizontal Bar Chart).
- **Data Table**
  - Columns: Rank, Role Title (Link to Detail), Postings Count, Trend (%), Action Link.
- **Skill Grid**
  - Cards: Category Badge, Name, Count, Demand Tier Badge.

---

## 6. Role Detail Page (`Rankings/RoleDetail.cshtml`)
- **Stat Cards**
  - Total Openings, Market Avg Salary, Demand Score.
- **Charts**
  - Canvas ID: `demandTrendChart` (Line Chart).
  - Canvas ID: `roleSkillsChart` (Bar Chart).
- **Locked Content (Give-to-Get)**
  - Containers: "Salary by Experience", "Salary by Location".
  - Feature: Blurs and shows "Unlock" modal if user hasn't contributed.

---

## 7. Profile Settings (`Profile/Index.cshtml`)
- **Profile Sidebar**
  - Initials Circle, Name, Email, Stats (Saved, Skills).
- **Settings Form**
  - Action: `SaveProfile`.
  - Input: `DisplayName`.
  - Select: `ExperienceLevel`.
  - **Skill Pills Group**: `id="skill-pills"`. Checkboxes hidden, toggled by JS on pill click.
  - Button: "Save Profile Changes".

---

## 8. Salary Submission (`Salary/Submit.cshtml`)
- **Submission Form**
  - Action: `Submit`.
  - Inputs: `JobTitle`, `Location`, `Salary`.
  - Select: `Currency` (USD, EGP, SAR, AED, EUR).
  - Slider: `id="exp-slider"`, Value Display: `id="exp-value"`.
  - Textarea: `SkillsList` (Comma separated).
  - Button: "Submit Market Data".

---

## 9. Reusable Component: Job Card (`_JobCard.cshtml`)
- Title Link (Blue).
- Company Name (Gray).
- Location Icon/Text.
- Salary Row (Green, Monospace).
- Skill Pills (Max 4).
- Posted Date & Source Site.
- "View Details" Link.

---

### Technical Design Notes
1. **Utility First**: Redesign using standard Tailwind CSS classes where possible.
2. **HTMX compatibility**: Ensure `#job-results` and `hx-*` attributes remain on the interactive elements.
3. **JS Triggers**: `DOMContentLoaded` events for `Chart.js` and `Modal` initialization must be preserved.

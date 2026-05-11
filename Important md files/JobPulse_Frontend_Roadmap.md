# JobPulse — Frontend Redesign Roadmap
**Style:** Modern & Bold | Strong colors | Big typography | Tailwind CSS + HTMX + Chart.js

---

## Design System

### Color Palette
| Token | Value | Usage |
|---|---|---|
| `--color-primary` | `#FF4D00` | CTAs, active states, highlights |
| `--color-primary-dark` | `#CC3D00` | Hover states |
| `--color-surface` | `#0D0D0D` | Page background |
| `--color-surface-2` | `#171717` | Cards, sidebar |
| `--color-surface-3` | `#222222` | Inputs, hover rows |
| `--color-border` | `#2E2E2E` | Borders, dividers |
| `--color-text-primary` | `#F5F5F5` | Headlines, body |
| `--color-text-secondary` | `#888888` | Labels, meta |
| `--color-green` | `#22C55E` | Salary, success |
| `--color-amber` | `#F59E0B` | Warnings, trend badges |

### Typography
| Role | Font | Weight |
|---|---|---|
| Display / Hero | `Syne` (Google Fonts) | 800 |
| Headings | `Syne` | 700 |
| Body / UI | `DM Sans` | 400 / 500 |
| Monospace (salary) | `JetBrains Mono` | 500 |

### Spacing Scale
Use Tailwind's default scale. Key breakpoints: `md` (768px), `lg` (1024px), `xl` (1280px).

---

## File Structure

```
Views/
├── Shared/
│   ├── _Navbar.cshtml          ← Global navbar
│   └── _JobCard.cshtml         ← Reusable job card
├── Home/
│   └── Index.cshtml            ← Landing page
├── Jobs/
│   ├── Index.cshtml            ← Search + filters
│   └── Detail.cshtml           ← Job detail
├── Rankings/
│   ├── Index.cshtml            ← Market rankings
│   └── RoleDetail.cshtml       ← Role detail
├── Profile/
│   └── Index.cshtml            ← Profile settings
└── Salary/
    └── Submit.cshtml           ← Salary submission
```

---

## Pages & Components Checklist

### ✅ Phase 1 — Shared Components
- [ ] `_Navbar.cshtml` — Brand, nav links, auth-aware dropdown
- [ ] `_JobCard.cshtml` — Reusable card with skills, salary, date
- [ ] `_Footer.cshtml` — Simple footer with links (new)

### ✅ Phase 2 — Landing Page (`Home/Index`)
- [ ] Hero section — big headline, search input, submit button
- [ ] Stat counters — 4 animated cards with `data-count-to`
- [ ] Trending roles chart — `trendingRolesChart` (horizontal bar, Chart.js)
- [ ] Skill cloud — loop of `/Rankings/RoleDetail?title=` links
- [ ] Job feed grid — grid of `_JobCard` partials
- [ ] Personalized section — blurred CTA if logged out
- [ ] Salary modal — `salary-modal`, "Contribute Now" → `/Salary/Submit`

### ✅ Phase 3 — Job Search Page (`Jobs/Index`)
- [ ] Sidebar filters — query, location, experience radios, salaryMin, clear button
- [ ] Results container `#job-results` — HTMX target, sort select, pagination

### ✅ Phase 4 — Job Detail Page (`Jobs/Detail`)
- [ ] Header card — title, company, location, date, external link, save button
- [ ] Market demand progress bar
- [ ] About section + skill links
- [ ] Salary intelligence range bar
- [ ] Sidebar — `skillDemandChart` radar chart, similar jobs list

### ✅ Phase 5 — Rankings Page (`Rankings/Index`)
- [ ] `topRolesChart` horizontal bar chart
- [ ] Rankings data table with rank, title, count, trend %, action
- [ ] Skill grid with category badge, name, count, demand tier

### ✅ Phase 6 — Role Detail Page (`Rankings/RoleDetail`)
- [ ] Stat cards — openings, avg salary, demand score
- [ ] `demandTrendChart` line chart + `roleSkillsChart` bar chart
- [ ] Locked/blurred salary sections with unlock modal

### ✅ Phase 7 — Profile Settings (`Profile/Index`)
- [ ] Sidebar — initials circle, name, email, stats
- [ ] Settings form — DisplayName, ExperienceLevel, skill pills `#skill-pills`

### ✅ Phase 8 — Salary Submission (`Salary/Submit`)
- [ ] Form — JobTitle, Location, Salary, Currency select, experience slider `#exp-slider`, SkillsList textarea

---

## Key Rules to Preserve (Never Break These)

| Element | Rule |
|---|---|
| `#job-results` | Must remain the HTMX swap target — do not rename |
| `hx-*` attributes | Keep all HTMX attributes on their original elements |
| `name="query"` | Sidebar search input name must stay `query` |
| `name="location"`, `name="experienceLevel"`, `name="salaryMin"` | Filter names must match model binding |
| `id="save-btn"` | Job detail save button ID must stay for JS toggle |
| `id="trendingRolesChart"` | Canvas ID must stay for Chart.js init |
| `id="topRolesChart"` | Canvas ID must stay |
| `id="demandTrendChart"`, `id="roleSkillsChart"` | Canvas IDs must stay |
| `id="skillDemandChart"` | Radar chart canvas ID must stay |
| `id="skill-pills"` | Profile form pill group ID must stay for JS |
| `id="exp-slider"`, `id="exp-value"` | Salary slider IDs must stay |
| `id="salary-modal"` | Modal ID must stay |
| `DOMContentLoaded` JS blocks | Keep all Chart.js and modal init scripts intact |
| `action="SaveProfile"` | Form action must stay |
| `action="Submit"` | Salary form action must stay |

---

## Implementation Order (Recommended)

```
1. Design tokens → CSS variables in _Layout.cshtml
2. _Navbar + _Footer (shared across all pages)
3. _JobCard (used in Home + Jobs pages)
4. Home/Index (most complex, tests all components)
5. Jobs/Index (HTMX live search)
6. Jobs/Detail (charts + save button JS)
7. Rankings/Index (table + chart)
8. Rankings/RoleDetail (locked content logic)
9. Profile/Index (skill pills JS)
10. Salary/Submit (slider JS)
```

---

## CDN Dependencies (add to `_Layout.cshtml`)

```html
<!-- Fonts -->
<link href="https://fonts.googleapis.com/css2?family=Syne:wght@700;800&family=DM+Sans:wght@400;500&family=JetBrains+Mono:wght@500&display=swap" rel="stylesheet">

<!-- Tailwind CSS -->
<script src="https://cdn.tailwindcss.com"></script>

<!-- Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- HTMX -->
<script src="https://unpkg.com/htmx.org@1.9.10"></script>

<!-- Flowbite (optional dropdowns/modals) -->
<link href="https://cdnjs.cloudflare.com/ajax/libs/flowbite/2.2.1/flowbite.min.css" rel="stylesheet">
<script src="https://cdnjs.cloudflare.com/ajax/libs/flowbite/2.2.1/flowbite.min.js"></script>
```

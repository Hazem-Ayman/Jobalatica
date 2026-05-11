# JobPulse — UI Bug Fixes (May 2026)

> This document lists targeted fixes for 5 confirmed UI bugs. Each fix includes the problem, root cause, and exact implementation guidance.

---

## Bug 1 — Sort Dropdown: Text Case Inconsistency

**Location:** Market page → Sort Hierarchy dropdown (top right of results)

**Problem:** The trigger label reads `RECENT FIRST` in all-caps (matching the site's label style), but the dropdown options render as Title Case ("Recent First", "Salary Delta", "Alpha Vector"). This inconsistency breaks the typographic contract of the design system.

**Rule:** All dropdown *options* must match the trigger label style — **ALL CAPS** with `letter-spacing: 0.1em` using Inter. The selected option text displayed inside the trigger must also be all-caps.

**Fix:**

```html
<!-- WRONG -->
<option value="recent">Recent First</option>
<option value="salary">Salary Delta</option>
<option value="alpha">Alpha Vector</option>

<!-- CORRECT -->
<option value="recent">RECENT FIRST</option>
<option value="salary">SALARY DELTA</option>
<option value="alpha">ALPHA VECTOR</option>
```

```css
/* Apply to all select elements and custom dropdown list items */
select option,
.dropdown-option {
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-family: Inter, sans-serif;
  font-size: 11px;
  font-weight: 400;
}
```

> **Note on search/keyword filter:** The same rule applies to the keyword filter input — when displaying matched job titles as suggestions or results, the case must match the stored data exactly (Title Case for job titles like "Product Manager"). Do **not** force all-caps on job title results — only on UI labels and dropdown options.

---

## Bug 2 — Region Dropdown: White/Light Background (Theme Break)

**Location:** Market page → Filter sidebar → "REGION" select dropdown

**Problem:** When the Region dropdown is opened, the native browser `<select>` renders with a **white background and dark text**, which completely breaks the dark theme. This is caused by using a native `<select>` element without overriding its system styles — browsers do not reliably allow full styling of native selects.

**Root Cause:** Native `<select>` dropdowns inherit OS/browser chrome. On Windows and most browsers, the open state cannot be fully styled with CSS alone.

**Fix:** Replace the native `<select>` with a custom Flowbite dropdown (already a dependency) or a CSS-only custom select.

```html
<!-- Replace native <select> with a custom dropdown -->
<div class="origin-select-wrapper" id="region-select">
  <button
    class="origin-input w-full flex justify-between items-center px-4 py-3"
    onclick="toggleDropdown('region-dropdown')"
  >
    <span id="region-label" class="text-white/60 text-xs tracking-widest uppercase">ALL REGIONS</span>
    <svg class="w-4 h-4 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
    </svg>
  </button>

  <div id="region-dropdown" class="origin-dropdown hidden absolute z-50 mt-1 w-full">
    <div class="glass-card py-2">
      <button class="dropdown-option" onclick="selectRegion('ALL REGIONS')">ALL REGIONS</button>
      <button class="dropdown-option" onclick="selectRegion('RIYADH')">RIYADH</button>
      <button class="dropdown-option" onclick="selectRegion('JEDDAH')">JEDDAH</button>
      <button class="dropdown-option" onclick="selectRegion('DUBAI')">DUBAI</button>
      <button class="dropdown-option" onclick="selectRegion('REMOTE')">REMOTE</button>
    </div>
  </div>
</div>
```

```css
.origin-dropdown {
  position: absolute;
  width: 100%;
}

.origin-dropdown .glass-card {
  background: #1a1b1e; /* Slightly lighter than bg for contrast */
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  backdrop-filter: blur(20px);
}

.dropdown-option {
  display: block;
  width: 100%;
  padding: 10px 16px;
  text-align: left;
  font-family: Inter, sans-serif;
  font-size: 11px;
  font-weight: 400;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.6);
  background: transparent;
  border: none;
  cursor: pointer;
  transition: color 0.15s, background 0.15s;
}

.dropdown-option:hover {
  color: #ffffff;
  background: rgba(132, 125, 255, 0.1);
}
```

```js
function toggleDropdown(id) {
  document.getElementById(id).classList.toggle('hidden');
}

function selectRegion(value) {
  document.getElementById('region-label').textContent = value;
  document.getElementById('region-dropdown').classList.add('hidden');
  // Trigger HTMX filter refresh here
}
```

---

## Bug 3 — Homepage Bar Chart: Too Small / Unreadable

**Location:** Homepage → "Collective Intelligence" section → bar chart

**Problem:** The Chart.js bar chart on the homepage is rendered at a very small size — bars are thin, labels are illegible, and the chart fails to communicate data clearly. Compare to the Rankings page (Demand Velocity) where the same chart renders at the correct, full-width size.

**Root Cause:** The chart container on the homepage likely has no explicit height set, causing Chart.js to collapse to a minimal size. Chart.js requires an explicit container height when `maintainAspectRatio` is false.

**Fix:**

```html
<!-- Ensure the container has an explicit height -->
<div class="glass-card p-6" style="min-height: 320px;">
  <canvas id="homepageChart"></canvas>
</div>
```

```js
// In the chart initialization for the homepage chart
const homepageChart = new Chart(ctx, {
  type: 'bar',
  data: { ... },
  options: {
    indexAxis: 'y',              // Horizontal bars
    maintainAspectRatio: false,  // REQUIRED — lets container control height
    responsive: true,
    plugins: {
      legend: { display: false }
    },
    scales: {
      x: {
        display: false,
        grid: { display: false }
      },
      y: {
        ticks: {
          color: 'rgba(255,255,255,0.5)',
          font: {
            family: 'Inter',
            size: 11,            // Minimum readable size
          }
        },
        grid: { display: false }
      }
    }
  }
});
```

```css
/* Ensure the canvas fills its container */
#homepageChart {
  width: 100% !important;
  min-height: 300px;
}
```

> **Reference:** The Rankings/Demand Velocity page chart is correctly sized. Use its container dimensions as the baseline for all chart instances.

---

## Bug 4 — Dashboard Sidebar: "SAVED SIGNALS" Text Too Small

**Location:** Dashboard page → Left sidebar navigation

**Problem:** The "SAVED SIGNALS" nav item text is rendering significantly smaller than "SETTINGS" above it, creating a mismatched, unprofessional sidebar. Both items should be identical in size, weight, and tracking.

**Root Cause:** The two nav items likely have different CSS classes or one is missing the shared `sidebar-nav-item` class.

**Fix:** Standardize both nav items to the same class:

```html
<!-- WRONG — mismatched markup -->
<a class="sidebar-nav-item text-sm">SETTINGS</a>
<a class="sidebar-nav-item text-xs font-light">SAVED SIGNALS</a>

<!-- CORRECT — identical markup for both -->
<a class="sidebar-nav-item" href="/dashboard/settings">
  <span class="nav-dot"></span>
  SETTINGS
</a>
<a class="sidebar-nav-item active" href="/dashboard/saved">
  <span class="nav-dot"></span>
  SAVED SIGNALS
</a>
```

```css
.sidebar-nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 16px;
  font-family: Inter, sans-serif;
  font-size: 11px;           /* Fixed — same for all items */
  font-weight: 400;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.5);
  border-radius: 9999px;
  transition: color 0.2s, background 0.2s;
  cursor: pointer;
}

.sidebar-nav-item:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.05);
}

.sidebar-nav-item.active {
  color: #847dff;
  background: rgba(132, 125, 255, 0.12);
}

.nav-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
  opacity: 0.6;
  flex-shrink: 0;
}
```

---

## Bug 5 — Nav User Dropdown: Wrong Items + Broken Links

**Location:** Global navigation → User email pill dropdown (top right)

**Problem (3 parts):**

**5a.** "DASHBOARD" appears as a purple-highlighted link inside the dropdown — it should NOT be there. Dashboard is already a top-level nav item. Having it duplicated inside the user dropdown is redundant and incorrect.

**5b.** "SAVED" does not navigate anywhere (broken link).

**5c.** "SIGN OUT" does not function (broken action).

---

### Fix 5a — Remove "DASHBOARD" from the dropdown

The dropdown should only contain account-level actions, not navigation duplicates.

```html
<!-- WRONG -->
<div class="user-dropdown">
  <a href="/dashboard" class="text-purple-400">DASHBOARD</a>  <!-- REMOVE THIS -->
  <a href="/saved">SAVED</a>
  <button>SIGN OUT</button>
</div>

<!-- CORRECT -->
<div class="user-dropdown glass-card py-2">
  <a class="dropdown-option" href="/dashboard/settings">SETTINGS</a>
  <a class="dropdown-option" href="/dashboard/saved">SAVED</a>
  <button class="dropdown-option danger" onclick="signOut()">SIGN OUT</button>
</div>
```

---

### Fix 5b — Fix "SAVED" link

```html
<!-- Ensure the href points to the correct route -->
<a class="dropdown-option" href="/Dashboard/Saved">SAVED</a>
```

If using ASP.NET Core routing:
```html
<a class="dropdown-option" asp-controller="Dashboard" asp-action="Saved">SAVED</a>
```

---

### Fix 5c — Fix "SIGN OUT" action

```html
<!-- If using ASP.NET Identity -->
<form method="post" asp-controller="Account" asp-action="Logout" id="logout-form">
  @Html.AntiForgeryToken()
  <button type="submit" class="dropdown-option danger">SIGN OUT</button>
</form>
```

```css
/* Sign out gets a distinct but subtle color — red/danger, not purple */
.dropdown-option.danger {
  color: rgba(255, 80, 80, 0.8);
}

.dropdown-option.danger:hover {
  color: rgb(255, 100, 100);
  background: rgba(255, 80, 80, 0.08);
}
```

---

## Summary Checklist

| # | Bug | Location | Status |
|:--|:----|:---------|:-------|
| 1 | Sort dropdown options not all-caps | Market → Sort Hierarchy | Fix above |
| 2 | Region dropdown white background (theme break) | Market → Filter Sidebar | Replace native `<select>` |
| 3 | Homepage chart too small / unreadable | Homepage → Collective Intelligence | Set explicit height + `maintainAspectRatio: false` |
| 4 | "SAVED SIGNALS" text too small vs "SETTINGS" | Dashboard → Sidebar | Standardize `.sidebar-nav-item` class |
| 5a | "DASHBOARD" incorrectly in user dropdown | Nav → User Pill | Remove duplicate link |
| 5b | "SAVED" link broken | Nav → User Pill | Fix route/href |
| 5c | "SIGN OUT" not functioning | Nav → User Pill | Wire to logout form with CSRF token |

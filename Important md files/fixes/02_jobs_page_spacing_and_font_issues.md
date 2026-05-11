# Issue #2 — Jobs Page Spacing & Typography Inconsistency

## Page: Jobs / Job Marketplace (`/jobs`)

## Problem Summary

The Jobs page has two distinct problems: excessive vertical spacing between job listing cards that makes the page feel empty and hard to scan, and a typography inconsistency in the Location filter dropdown that breaks the visual language of the rest of the page.

---

## Specific Issues

### 1. Excessive Vertical Spacing Between Job Cards

Each job listing card has very large top and bottom padding/margins. The result is that only 3–4 listings are visible at a time on a standard screen, forcing users to scroll significantly more than necessary.

**Problems:**
- Cards feel visually "floating" with too much whitespace between them.
- The separator lines between cards are barely visible, and the spacing makes it unclear where one card ends and the next begins without the separator doing its job.
- Users cannot scan multiple listings at a glance — a core behavior in job searching. Dense, compact cards (like LinkedIn, Wuzzuf, or Glassdoor) are the industry standard because they allow faster comparison.

**What to fix:**
- Reduce the vertical padding inside each card. Current feel is approximately `40–60px` top/bottom padding per card. Recommended: `20–28px`.
- Reduce the gap/margin between cards from the current generous spacing to something tighter (e.g., `0` gap with just the border separator, or `8–12px` if cards have a background).

---

### 2. Location Dropdown — Wrong Font

The `LOCATION` filter label and its dropdown (`All Locations ▾`) use a **different typeface** from the rest of the page.

**What the rest of the page uses:**
- A serif display font (appears to be a variant of Playfair Display or similar editorial serif) for headings and UI labels.
- A clean sans-serif (appears to be Inter or similar) for body text and filter labels.

**What the Location dropdown uses:**
- The native browser default dropdown (`<select>` element), which renders in the OS system font — typically `Arial`, `Helvetica`, or `-apple-system`. This is visually inconsistent with the custom typography used everywhere else.

**Problems:**
- The dropdown looks like it belongs to a different, older page.
- The font weight, letter spacing, and rendering all differ from the styled filter labels above it (`KEYWORDS`, `EXPERIENCE LEVEL`, `MIN. SALARY`).
- The `All Locations` option text appears heavier/different than intended.

**What to fix:**
- Replace the native `<select>` with a custom-styled dropdown component that uses the correct font family, font size, and border styling consistent with the rest of the filter sidebar.
- Alternatively, apply explicit CSS to the `<select>` element: `font-family`, `font-size`, `color`, and `appearance: none` with a custom chevron.

---

### 3. Filter Sidebar — Label Sizing Inconsistency (Minor)

The filter section labels (`KEYWORDS`, `LOCATION`, `EXPERIENCE LEVEL`, `MIN. SALARY (USD/YR)`) are styled in small-caps/uppercase tracking — which is correct. However their visual weight relative to the input fields below them creates a slightly unbalanced hierarchy.

**What to fix (optional):**
- Ensure consistent spacing between each filter label and its corresponding input/control.
- Ensure the `CLEAR ALL FILTERS` link at the bottom is visually distinct enough to be noticed but not so prominent it competes with the filter controls.

---

## Affected Elements

| Element | Location | Severity |
|---|---|---|
| Excessive card padding/spacing | Job listing cards | High |
| Location dropdown font mismatch | Filter sidebar | High |
| Gap between cards | Job listing list | Medium |
| Filter label-to-input spacing | Filter sidebar | Low |
| CLEAR ALL FILTERS visibility | Filter sidebar bottom | Low |

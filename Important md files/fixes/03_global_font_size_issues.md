# Issue #3 — Global Font Size Too Small Across All Pages

## Scope: All Pages (Home, Jobs, Rankings)

## Problem Summary

The base font size used across the entire application is too small for comfortable reading. Body text, labels, tags, and secondary information are all rendered at sizes that strain readability — particularly on larger screens and for users who are not on high-DPI displays.

---

## Specific Issues

### 1. Body Text — Too Small

**Affected areas:**
- Homepage: subtitle text under the main heading (`A living matrix of professional worth. Analyze signals on demand...`) appears to be around **12–13px**.
- Jobs page: job listing detail lines (`LinkedIn · Full-time`, source/location sub-labels like `GOOGLE · JEDDAH`) appear at approximately **11–12px**.
- Filter sidebar: all filter labels and input placeholder text.

**Industry standard:** Body text for web applications should be a minimum of **15–16px**, with secondary/meta text no smaller than **13px**. Text below 13px is difficult to read without zooming on standard displays.

---

### 2. Skill/Tag Chips — Too Small

The technology tag chips on job cards (e.g., `AWS`, `Jenkins`, `SQL`, `Python`, `GraphQL`) are rendered at approximately **10–11px** with tight padding. These are interactive elements (or at minimum, scannable content) and should be legible at a glance.

**What to fix:**
- Increase chip font size to **12–13px** minimum.
- Increase horizontal padding inside chips slightly to give text more breathing room.

---

### 3. Navigation Bar — Label Sizes Are Fine But Inconsistent

The top navigation (`Home`, `Jobs`, `Rankings`) appears correctly sized. However, the "MARKET INTELLIGENCE" section label on the homepage and "LIVE MARKET" on the jobs page use very small uppercase tracking text (approximately **10px**) as section identifiers. While this is an intentional design choice (editorial style), at this size they may be missed entirely on first scan.

**Recommendation:** Increase these category labels to **11–12px** if they are meant to be read, or treat them purely as decorative if they are not critical to user orientation.

---

### 4. Stats Widget — Value Text Is Fine, Label Text Is Not

In the Live Signals widget on the homepage:
- The large numbers (`12,392`, `849`, etc.) are well-sized and readable.
- The sub-labels (`ACTIVE SIGNALS`, `ROLES TRACKED`, `MARKETS`, `COMPANIES`) are approximately **9–10px** — too small to read comfortably without leaning in.

**What to fix:**
- Increase widget sub-labels to **11–12px**.

---

## Recommended Global Type Scale

Use this as a baseline reference for the agent when fixing font sizes:

| Role | Recommended Size |
|---|---|
| Page heading (H1) | 40–56px |
| Section heading (H2) | 28–36px |
| Card title | 18–22px |
| Body text | 15–16px |
| Secondary / meta text | 13–14px |
| Labels / chips / tags | 12–13px |
| Micro labels (section IDs) | 11–12px |
| Minimum readable size | 11px (hard floor) |

---

## Affected Elements

| Element | Estimated Current Size | Recommended Size | Severity |
|---|---|---|---|
| Homepage subtitle body text | ~12–13px | 15–16px | High |
| Job card sub-labels (company, location) | ~11–12px | 13–14px | High |
| Skill/tag chips | ~10–11px | 12–13px | High |
| Live Signals widget sub-labels | ~9–10px | 11–12px | Medium |
| Section identifier labels (MARKET INTELLIGENCE, LIVE MARKET) | ~10px | 11–12px | Medium |
| Filter sidebar labels and inputs | ~12px | 13–14px | Medium |

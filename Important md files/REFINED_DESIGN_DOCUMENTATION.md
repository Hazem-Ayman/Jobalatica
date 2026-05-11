# JobPulse — Refined Design & Visual Identity Documentation

> **Revision Note (May 2026):** This document supersedes the previous version. Six systemic inconsistencies were identified across the live pages and are corrected here with explicit, enforceable rules. Each fix is marked with `[FIX]` for clarity.

---

## 1. Design Philosophy: "The Professional Matrix"
The design communicates **authority, innovation, and transparency** through a sophisticated "AI Lab" aesthetic.
- **Minimalism**: Removing unnecessary borders; whitespace and padding create breathing room.
- **High Fidelity**: Strategic use of transparency (glassmorphism) and lighting (radial glows).
- **Technical Depth**: Monospaced-style tracking and uppercase labels suggest precision.

> **Core Principle:** Every page — homepage, market, rankings, dashboard — must feel like it belongs to the same product. Visual language is non-negotiable and must be applied uniformly.

---

## 2. Color Palette
The palette uses a deep, dark base with vibrant professional accents.

| Role | Color Name | Hex Code | Usage |
| :--- | :--- | :--- | :--- |
| **Background** | Origin Dark | `#0f1011` | Primary surface for all pages. |
| **Surface** | Glass Card | `rgba(255,255,255,0.03)` | Background for cards and containers. |
| **Primary Text** | Solid White | `#ffffff` | Headings and primary data. |
| **Secondary Text** | Muted White | `rgba(255,255,255,0.6)` | Descriptions and labels. |
| **Accent 1** | Origin Blue | `#195f97` | Primary brand identifier. |
| **Accent 2** | Light Purple | `#847dff` | Interactive highlights, active states, **all chart fills**. |
| **Accent 3** | Cyan | `#00b3dd` | Success states, trend delta indicators (e.g. `+12%`). |

### [FIX 1] — Chart Color Standardization

**Problem:** The demand/salary bar charts used `origin-light-purple` (`#847dff`) on the homepage but switched to `origin-blue` (`#195f97`) on the Rankings/Demand Velocity page, making it appear as two different products.

**Rule:** All Chart.js bar charts site-wide **must** use `origin-light-purple` (`#847dff`) as the primary fill, with `rgba(132, 125, 255, 0.15)` as the transparent area fill. `origin-blue` is reserved for brand logo accents and external link highlights only.

```js
// CORRECT — apply this chart theme globally
const chartTheme = {
  backgroundColor: 'rgba(132, 125, 255, 0.7)',
  borderColor: '#847dff',
  borderWidth: 1,
  borderRadius: 4,
};
```

---

## 3. Typography System

### Headings (Serif)
- **Font**: Georgia (System fallback for Lyondisplay).
- **Weight**: `font-weight: 300` (Light). **This must never be overridden.**
- **Style**: `heading-massive`.
- **Characteristics**: Large font sizes (up to 100px), tight letter-spacing (`-0.04em`).
- **Italic Emphasis**: The **second line** of a two-line hero heading must be italicized and rendered in `rgba(255,255,255,0.5)` to create the light/muted contrast. This applies to ALL hero headings across all pages.

### [FIX 2] — Heading Weight Consistency

**Problem:** The hero heading on the homepage ("Know your worth") rendered heavier than the same heading on the Market page ("Collective Intelligence"), suggesting `font-weight` was being overridden by a parent container or Tailwind utility class.

**Rule:** Add `!important` to the heading weight declaration in the global stylesheet and audit all page-level containers for conflicting `font-bold` or `font-semibold` Tailwind classes on heading wrappers.

```css
/* In _Layout.cshtml global styles — non-negotiable */
.heading-massive {
  font-family: Georgia, serif;
  font-weight: 300 !important;
  letter-spacing: -0.04em;
  line-height: 1.05;
}
```

### [FIX 3] — Subheading Hierarchy Rule

**Problem:** Some pages used the correct italic serif treatment for subheadings (e.g. *"in real-time."*), while other pages (Market page) jumped directly to an all-caps sans-serif label for what is a semantically equivalent subheading ("REAL-TIME SIGNALS FROM THE DEVELOPER MATRIX").

**Rule:** Apply a strict two-tier subheading system:

| Tier | Usage | Style |
|:---|:---|:---|
| **Hero Subheading** | Second line of a hero heading block | Italic Georgia, `rgba(255,255,255,0.5)`, same size as heading |
| **Section Label** | Metadata, filters, column headers, small descriptors | `Inter`, uppercase, `letter-spacing: 0.3em`, `rgba(255,255,255,0.4)`, `font-size: 11px` |

The "REAL-TIME SIGNALS FROM THE DEVELOPER MATRIX" line on the Market page is a **Hero Subheading** and must be styled accordingly — italic, serif, muted white — not as a section label.

### Body & UI (Sans-Serif)
- **Font**: Inter.
- **Weights**: 300 (Light) to 600 (Bold).
- **Usage**: Body copy, buttons, form labels, small metadata.

---

## 4. Key Components

### Glass Cards (`.glass-card`)
- **Background**: `rgba(255, 255, 255, 0.03)`.
- **Blur**: `backdrop-filter: blur(20px)`.
- **Border**: `1px solid rgba(255, 255, 255, 0.08)`.
- **Corner Radius**: `16px` (Standard cards) / `32px` (Section containers).

### [FIX 4] — Glass Card Treatment on Inner Pages

**Problem:** The Market page's filter sidebar and job listing cards appeared flat and more opaque than homepage cards. The glassmorphism effect was visually absent, making inner pages feel utilitarian compared to the homepage.

**Rule:** The `.glass-card` class must be applied uniformly to **every** card and sidebar container across all pages. Do not create page-specific card classes. Audit `Market.cshtml`, `Rankings.cshtml`, and `Dashboard.cshtml` for any elements using `bg-gray-*`, `bg-zinc-*`, or `background-color: #1a1a1a` equivalents and replace them with the `.glass-card` class.

```css
/* The one card style. No exceptions. */
.glass-card {
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;
}
```

### [FIX 5] — Button Shape Consistency (Pill vs. Rectangle)

**Problem:** The primary CTA buttons ("ANALYZE →") correctly used full pill shapes (`border-radius: 9999px`). However, the seniority filter toggles on the Market page ("ENTRY", "MID", "SENIOR", "LEAD") used near-rectangular shapes with minimal border-radius, breaking the rounded design language.

**Rule:** All interactive button-style elements — including filter toggles, tab selectors, and tag chips — must use a consistent border-radius. Apply the following scale:

| Element Type | Border Radius |
|:---|:---|
| Primary CTA buttons | `border-radius: 9999px` (full pill) |
| Filter toggles / Seniority selectors | `border-radius: 9999px` (full pill) |
| Skill tag chips | `border-radius: 9999px` (full pill) |
| Dropdown selects | `border-radius: 12px` |
| Input fields | `border-radius: 12px` |

```html
<!-- CORRECT seniority toggle markup -->
<button class="origin-button px-4 py-2 rounded-full text-xs tracking-widest uppercase">
  ENTRY
</button>
```

### Origin Buttons (`.origin-button`)
- **Shape**: Pill (Full rounded, `border-radius: 9999px`).
- **Background**: `rgba(255, 255, 255, 0.1)` default / `rgba(255, 255, 255, 0.2)` on hover.
- **Active/Selected State**: Background `rgba(132, 125, 255, 0.25)`, border `rgba(132, 125, 255, 0.5)`.
- **Hover**: Transitions to `0.3` opacity with a subtle upward lift (`translateY(-1px)`).
- **Iconography**: CTA buttons always paired with an arrow (→).

### Origin Inputs (`.origin-input`)
- **Style**: Minimalist, `border-radius: 12px`, `border: 1px solid rgba(255,255,255,0.08)`.
- **Focus State**: Glows with `box-shadow: 0 0 0 2px rgba(132, 125, 255, 0.4)`.

---

## 5. Visual Elements & Motion

### [FIX 6] — Radial Glows on All Pages

**Problem:** The atmospheric background glows (blue top-center, purple bottom-left) were only visible on the homepage. Inner pages (Market, Rankings, Dashboard) rendered a completely flat `#0f1011` background, stripping away the depth and atmosphere that defines the aesthetic.

**Rule:** The `.origin-bg-glow` container must be present in `_Layout.cshtml` (the shared layout file), not in individual page views. This guarantees every page inherits the atmospheric background automatically.

```html
<!-- Place ONCE in _Layout.cshtml, inside <body>, before all content -->
<div class="origin-bg-glow" aria-hidden="true">
  <div class="glow-top-center"></div>
  <div class="glow-bottom-left"></div>
</div>
```

```css
/* In global styles */
.origin-bg-glow {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
  overflow: hidden;
}

.glow-top-center {
  position: absolute;
  top: -20%;
  left: 50%;
  transform: translateX(-50%);
  width: 800px;
  height: 600px;
  background: radial-gradient(ellipse, rgba(25, 95, 151, 0.15) 0%, transparent 70%);
}

.glow-bottom-left {
  position: absolute;
  bottom: -10%;
  left: -10%;
  width: 700px;
  height: 500px;
  background: radial-gradient(ellipse, rgba(132, 125, 255, 0.12) 0%, transparent 70%);
}

/* Ensure all page content sits above the glow layer */
body > *:not(.origin-bg-glow) {
  position: relative;
  z-index: 1;
}
```

### Data Visualization
- **Charts**: Custom Chart.js theme using **`origin-light-purple` only** (see Fix 1).
- **Animations**:
  - **Pulsing**: Small cyan (`#00b3dd`) dots next to "Live" indicators.
  - **Animated Counters**: Stats count up from 0 on page load using `IntersectionObserver`.
  - **Fade-in**: Hero text uses CSS transitions to appear smoothly on load.

---

## 6. UX Logic Implementation

### "Give-to-Get" Salary Model
Advanced salary intelligence is visually locked behind a blurred overlay for unauthenticated users.
- **Effect**: `filter: blur(8px); opacity: 0.2;`
- **Trigger**: Unlocked upon submission of a `SalaryReport` by the authenticated user.

### Live Matrix Search
- **HTMX Integration**: Search sidebar uses HTMX to update `#job-results` without page refreshes.
- **Feedback**: Search input triggers on `keyup delay:400ms`.

---

## 7. Technical Implementation Layer
- **Tailwind CSS**: Used for 95% of styling via custom configuration in `_Layout.cshtml`.
- **Flowbite**: Powers interactive Modals (Salary Popup) and Dropdowns.
- **HTMX**: Handles the Market feed engine.
- **Chart.js**: Visualizes demand and skill velocity.

---

## 8. Consistency Checklist (Audit Against Every New Page)

Before shipping any new page or component, verify:

- [ ] Background glows are visible (inherited from `_Layout.cshtml`)
- [ ] All cards use `.glass-card` — no flat/opaque alternatives
- [ ] All buttons and toggles use `border-radius: 9999px`
- [ ] All bar charts use `origin-light-purple` (`#847dff`) fill
- [ ] Hero headings use `font-weight: 300` Georgia serif
- [ ] Hero subheadings use italic serif + `rgba(255,255,255,0.5)` — not all-caps label style
- [ ] Section metadata labels use `Inter`, uppercase, `letter-spacing: 0.3em`
- [ ] Delta/trend values use Cyan (`#00b3dd`)
- [ ] Active/selected states use purple tint (`rgba(132, 125, 255, 0.25)`)

# Fix: Rankings Page Layout & Logic
> File: `Views/Rankings/Index.cshtml`

---

## Problem

The current layout shows the chart and the roles table side-by-side at the same visual weight. This is wrong — the page feels cluttered and the user doesn't know where to look first. The skills section is also crammed below without breathing room.

The correct reading flow should be:
```
1. Big hero chart (full width, dominates the page)
        ↓
2. Role rankings table (full width, below chart)
        ↓
3. Skills index (full width, below roles)
```

---

## Fix: Restructure the Page into 3 Clear Full-Width Sections

> **Style reference:** The site uses a warm cream background (`#E8E0D5`), heavy serif display font (Playfair Display or equivalent), coral/salmon accent (`#F25C5C`), black for all primary text, and uppercase spaced tracking labels in muted gray. All borders are thin `1px solid #C8C0B8`. No rounded corners beyond `2px`. No shadows. Spacing is generous and editorial.

---

### Global Page Wrapper

The page sits inside the shared layout. Add a page-level class to the outermost container that enforces the background and base typography:

```html
<div class="rankings-page" style="background: #E8E0D5; min-height: 100vh; font-family: 'DM Sans', sans-serif; color: #111;">
    <!-- Sections go here -->
</div>
```

---

### Section 1 — Chart (Full Width, Hero Treatment)

Remove the current side-by-side split. The chart gets its own full-width section with generous vertical padding. It should feel like the editorial centerpiece of the page.

```html
<!-- SECTION 1: Chart Hero -->
<section style="padding: 72px 64px; border-bottom: 1px solid #C8C0B8;">
    <div style="max-width: 1100px; margin: 0 auto;">

        <!-- Section label — matches site's "MARKET INTELLIGENCE" label style -->
        <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 16px;">
            <span style="display: inline-block; width: 10px; height: 10px; background: #F25C5C;"></span>
            <span style="font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888;">
                Postings Volume
            </span>
        </div>

        <!-- Heading — matches site's serif hero style -->
        <h1 style="font-family: 'Playfair Display', Georgia, serif; font-size: 52px; font-weight: 800; line-height: 1.05; color: #111; margin: 0 0 12px 0;">
            Role Rankings
        </h1>
        <p style="font-size: 14px; color: #888; margin: 0 0 48px 0;">
            Based on the last 30 days of market activity across 12,400+ verified postings.
        </p>

        <!-- Chart container — bordered box, no background fill (cream shows through) -->
        <div style="border: 1px solid #C8C0B8; padding: 40px;">
            <canvas id="topRolesChart" height="320"></canvas>
        </div>

    </div>
</section>
```

**Chart bar color:** Use `#111111` (black) for bars to match the homepage "Demand Velocity" chart style. No colored gradients.

---

### Section 2 — Roles Table (Full Width, Below Chart)

The roles table gets its own section. The Monthly/Quarterly toggle moves here, above the table — it belongs next to the data it filters, not floating on the chart.

```html
<!-- SECTION 2: Roles Table -->
<section style="padding: 72px 64px; border-bottom: 1px solid #C8C0B8;">
    <div style="max-width: 1100px; margin: 0 auto;">

        <!-- Section header row -->
        <div style="display: flex; align-items: flex-end; justify-content: space-between; margin-bottom: 40px;">
            <div>
                <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 12px;">
                    <span style="display: inline-block; width: 10px; height: 10px; background: #111;"></span>
                    <span style="font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888;">
                        Sorted by Demand
                    </span>
                </div>
                <h2 style="font-family: 'Playfair Display', Georgia, serif; font-size: 36px; font-weight: 800; color: #111; margin: 0;">
                    Top Roles
                </h2>
            </div>

            <!-- Monthly / Quarterly toggle — site button style: black filled vs outlined -->
            <div style="display: flex; gap: 0; border: 1px solid #111;">
                <button style="background: #111; color: #E8E0D5; font-size: 11px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; padding: 10px 20px; border: none; cursor: pointer;">
                    MONTHLY
                </button>
                <button style="background: transparent; color: #111; font-size: 11px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; padding: 10px 20px; border: none; border-left: 1px solid #111; cursor: pointer;">
                    QUARTERLY
                </button>
            </div>
        </div>

        <!-- Table — full width, no card background, editorial table style -->
        <table style="width: 100%; border-collapse: collapse;">
            <thead>
                <tr style="border-bottom: 1px solid #C8C0B8;">
                    <th style="text-align: left; font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888; padding: 12px 16px 12px 0;">#</th>
                    <th style="text-align: left; font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888; padding: 12px 16px;">Role Title</th>
                    <th style="text-align: left; font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888; padding: 12px 16px;">Postings</th>
                    <th style="text-align: left; font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888; padding: 12px 16px;">Trend</th>
                    <th style="padding: 12px 0 12px 16px;"></th>
                </tr>
            </thead>
            <tbody>
                @* Razor loop: @foreach (var role in Model.Rankings) *@
                @* Each row: border-bottom 1px solid #C8C0B8, row font-size 14px, title font-weight 700 *@
                @* "VIEW DETAILS →" link: font-size 11px, font-weight 700, letter-spacing 0.1em, text-transform uppercase, color #111 *@
            </tbody>
        </table>

    </div>
</section>
```

**Table row style reference (for Razor loop):**
```html
<tr style="border-bottom: 1px solid #C8C0B8;">
    <td style="padding: 20px 16px 20px 0; font-size: 13px; color: #888;">01</td>
    <td style="padding: 20px 16px; font-size: 15px; font-weight: 700; color: #111;">Product Manager</td>
    <td style="padding: 20px 16px; font-size: 14px; color: #111;">2,847</td>
    <td style="padding: 20px 16px; font-size: 13px; color: #888;">↑ +12%</td>
    <td style="padding: 20px 0 20px 16px; text-align: right;">
        <a href="#" style="font-size: 11px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: #111; text-decoration: none;">
            VIEW DETAILS →
        </a>
    </td>
</tr>
```

---

### Section 3 — Skills Index (Full Width, Below Roles)

The skills grid gets its own separated section with a proper header. The grid uses a `1px` gap technique — the parent has the border color as a background, and children have the page background, so the lines between cells appear automatically (matching the site's "Live Signals" 4-cell widget style).

```html
<!-- SECTION 3: Skills Index -->
<section style="padding: 72px 64px;">
    <div style="max-width: 1100px; margin: 0 auto;">

        <!-- Section header -->
        <div style="margin-bottom: 48px;">
            <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 12px;">
                <span style="display: inline-block; width: 10px; height: 10px; background: #F25C5C;"></span>
                <span style="font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888;">
                    Skill Matrices
                </span>
            </div>
            <h2 style="font-family: 'Playfair Display', Georgia, serif; font-size: 36px; font-weight: 800; color: #111; margin: 0 0 12px 0;">
                Industry Skills Index
            </h2>
            <p style="font-size: 14px; color: #888; max-width: 480px; margin: 0;">
                Every skill tracked by demand frequency, salary delta, and market velocity —
                so you know exactly where to invest your learning time.
            </p>
        </div>

        <!-- 4-column grid — gap line technique matching site widget style -->
        <div style="display: grid; grid-template-columns: repeat(4, 1fr); gap: 1px; background: #C8C0B8; border: 1px solid #C8C0B8;">
            @* Razor loop: @foreach (var skill in Model.TopSkills) *@
            @* Each card (see card template below) *@
        </div>

    </div>
</section>
```

**Skill card template (for Razor loop):**
```html
<div style="background: #E8E0D5; padding: 28px 24px;">
    <!-- Category label -->
    <p style="font-size: 10px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888; margin: 0 0 8px 0;">
        @skill.Category
    </p>
    <!-- Skill name -->
    <p style="font-size: 16px; font-weight: 700; color: #111; margin: 0 0 12px 0;">
        @skill.Name
    </p>
    <!-- Postings count -->
    <p style="font-size: 22px; font-weight: 800; color: #111; margin: 0 0 4px 0;">
        @skill.PostingsCount.ToString("N0")
    </p>
    <p style="font-size: 11px; color: #888; margin: 0 0 16px 0;">postings</p>
    <!-- Demand tier badge — matches site's ENTRY / LEAD tag style -->
    <span style="display: inline-block; border: 1px solid #111; font-size: 10px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; padding: 3px 8px; color: #111;">
        @skill.DemandTier
    </span>
</div>
```

---

## Summary of Changes

| What | Before | After |
|---|---|---|
| Chart | Side by side with table | Full width, hero section alone |
| Table | Crammed next to chart | Full width, own section below chart |
| Monthly/Quarterly toggle | Top-right corner of chart area | Above the table as a split-button |
| Skills section | Directly below table, no breathing room | Own full section with proper header and grid |
| Page reading flow | Two competing columns | Linear: Chart → Roles → Skills |
| Button style | Generic rounded dark buttons | Black/cream split-button with `1px solid #111` border |
| Table style | Card with background fill | Flat editorial table on cream, `1px` row separators only |
| Skills grid style | Simple gap grid | `1px` border-line grid matching site's widget aesthetic |
| Section labels | Generic headings | Colored square + uppercase tracking label + serif H2 |

# Issue #1 — Homepage Layout & Content Order

## Page: Home (`/`)

## Problem Summary

The homepage layout has a logical ordering problem. The two-column split layout presents content in a way that feels disconnected and unnatural for users scanning the page.

---

## Specific Issues

### 1. Hero Section — Content Hierarchy Is Off

The left column reads:

```
MARKET INTELLIGENCE  ← label
Know your worth      ← h1
in real-time.        ← h1 (italic continuation)
A living matrix of professional professional worth.  ← body (also has a typo: "professional" repeated twice)
Analyze signals on demand, skills, and value.        ← body
[Search input] [ANALYZE →]  ← CTA
```

**Problems:**
- The tagline under the heading (`A living matrix of professional professional worth.`) is redundant — it repeats the value prop from the next section verbatim. This is confusing and feels like placeholder copy that was never cleaned up.
- There is a **typo**: "A living matrix of professional **professional** worth." — "professional" is duplicated.
- The CTA (search bar + Analyze button) appears below two lines of description text. Users may not reach it before losing interest. The CTA should be closer to the headline.

### 2. Live Signals Widget — Floating Without Context

The right column shows a `LIVE SIGNALS` stats widget with 4 metrics:
- 12,392 Active Signals
- 849 Roles Tracked
- 52 Markets
- 1,199 Companies

**Problems:**
- The widget appears isolated on the right side with no heading, intro text, or label explaining why it's there or what it means for the user.
- There is no visual connection between the left CTA and the right widget — they feel like two separate pages placed side by side.
- The widget's date (`05.11.26`) is displayed in small, hard-to-notice text in the top-right corner of the widget with no label (e.g., "Last updated"). This is ambiguous.

### 3. Split Layout Divider

A hard vertical line divides the page into two equal columns. This is an unusual layout choice for a hero section:
- It creates visual competition between the left CTA and the right stats panel.
- Neither side feels like the primary focus.
- Standard best practice: hero content should have a clear primary zone, with supporting content clearly subordinate (e.g., smaller, lower, or right-aligned with reduced visual weight).

---

## Suggested Fix Order

1. Fix the typo: `professional professional` → `professional`
2. Remove the redundant tagline or rewrite it to be distinct from the Section 2 headline
3. Move the search/CTA bar immediately below the main headline (before the body text)
4. Give the right-side stats widget a brief label or heading (e.g., "Market Pulse" or "Live Data")
5. Add a small caption to the widget date: `Last updated: 05.11.26`
6. Consider replacing the hard vertical divider with whitespace or a softer visual separator

---

## Affected Elements

| Element | Location | Severity |
|---|---|---|
| Typo in body text | Left column, hero | High |
| Redundant tagline | Left column, hero | Medium |
| CTA position | Left column, below body | Medium |
| Stats widget — no context label | Right column | Medium |
| Hard vertical divider | Full page | Low |
| Date label missing | Widget top-right | Low |

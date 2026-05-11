# JobPulse — Light Mode Design System

> This document defines the official light mode for JobPulse. The light mode is **not** a simple color inversion of the dark mode. It is a deliberate re-expression of the same "Professional Matrix" identity using warm parchment tones, translucent glass surfaces, and ink-on-paper typography.

---

## 1. Design Philosophy: "Parchment Intelligence"

The light mode draws from **editorial print design** — think Financial Times, The Economist — combined with the same glassmorphism language of the dark theme. 

The key insight is: **glassmorphism on light backgrounds needs warm, creamy surfaces — not white.** Pure white destroys the depth effect. A warm off-white base (`#F4F1EC`) makes frosted-glass cards feel elevated and luxurious instead of flat.

| Dark Mode Feel | Light Mode Feel |
|:---|:---|
| Cinematic AI lab | Editorial intelligence briefing |
| Neon glows on black | Ink on parchment |
| White text on dark glass | Dark text on frosted cream glass |
| Blue/purple radial glows | Soft blue/purple radial glows at 8–10% opacity |

---

## 2. Light Mode Color Palette

| Role | Token Name | Value | Usage |
|:---|:---|:---|:---|
| **Page Background** | `lm-bg` | `#F4F1EC` | Warm parchment. The entire page base. |
| **Glass Surface** | `lm-surface` | `rgba(255,255,255,0.60)` | Cards, sidebars, containers. |
| **Surface Solid** | `lm-surface-solid` | `#FFFFFF` | Dropdowns, modals (no blur needed). |
| **Border Default** | `lm-border` | `rgba(0,0,0,0.08)` | Card borders, dividers. |
| **Border Strong** | `lm-border-strong` | `rgba(0,0,0,0.14)` | Input outlines, badge borders. |
| **Primary Text** | `lm-text-primary` | `#1a1a1a` | Headings, key data, prices. |
| **Secondary Text** | `lm-text-secondary` | `rgba(0,0,0,0.45)` | Body copy, company names, labels. |
| **Tertiary Text** | `lm-text-tertiary` | `rgba(0,0,0,0.28)` | Placeholder, tags, hints. |
| **Accent Blue** | `lm-blue` | `#195f97` | Brand dot, link highlights. |
| **Accent Purple** | `lm-purple` | `#6B64D8` | Focus rings, active states, charts. |
| **Accent Cyan** | `lm-cyan` | `#0099BB` | Delta indicators (`+12%`). |

### CSS Variable Declaration

Add this inside your `:root[data-theme="light"]` block:

```css
:root[data-theme="light"] {
  --lm-bg:              #F4F1EC;
  --lm-surface:         rgba(255, 255, 255, 0.60);
  --lm-surface-solid:   #FFFFFF;
  --lm-border:          rgba(0, 0, 0, 0.08);
  --lm-border-strong:   rgba(0, 0, 0, 0.14);
  --lm-text-primary:    #1a1a1a;
  --lm-text-secondary:  rgba(0, 0, 0, 0.45);
  --lm-text-tertiary:   rgba(0, 0, 0, 0.28);
  --lm-blue:            #195f97;
  --lm-purple:          #6B64D8;
  --lm-cyan:            #0099BB;
  --lm-glow-blue:       rgba(25, 95, 151, 0.08);
  --lm-glow-purple:     rgba(107, 100, 216, 0.07);
}
```

---

## 3. Typography in Light Mode

Typography rules are **identical** to the dark mode — same fonts, same weights, same tracking. Only the colors change.

| Element | Font | Weight | Color (Light Mode) |
|:---|:---|:---|:---|
| Hero heading | Georgia, serif | 300 | `#1a1a1a` |
| Hero italic subheading | Georgia, serif italic | 300 | `rgba(0,0,0,0.35)` |
| Section titles | Georgia, serif | 300 | `#1a1a1a` |
| Section labels | Inter, uppercase | 400 | `rgba(0,0,0,0.28)`, `letter-spacing: 0.3em` |
| Body copy | Inter | 300–400 | `rgba(0,0,0,0.45)` |
| Price / stat numbers | Georgia, serif | 300 | `#1a1a1a` |
| Tags / chips text | Inter, uppercase | 500 | `rgba(0,0,0,0.45)` |
| Delta indicators | Inter | 500 | `#0099BB` (Cyan) |

---

## 4. Components in Light Mode

### Glass Cards (`.glass-card`)

```css
[data-theme="light"] .glass-card {
  background: rgba(255, 255, 255, 0.60);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 0.5px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
}
```

> **Why 0.60 opacity?** At 0.03 (dark mode value) on a light background, cards would be invisible. 0.60 gives a frosted-glass feel while still showing the warm parchment background through the blur.

---

### Origin Buttons (`.origin-button`)

The CTA button inverts: dark pill on light background.

```css
[data-theme="light"] .origin-button {
  background: #1a1a1a;
  color: #F4F1EC;
  border: none;
  border-radius: 9999px;
  padding: 12px 24px;
  font-family: Inter, sans-serif;
  font-size: 11px;
  font-weight: 500;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  transition: opacity 0.2s, transform 0.2s;
}

[data-theme="light"] .origin-button:hover {
  opacity: 0.8;
  transform: translateY(-1px);
}
```

---

### Filter/Toggle Buttons (seniority, type selectors)

```css
[data-theme="light"] .filter-toggle {
  background: rgba(255, 255, 255, 0.55);
  border: 0.5px solid rgba(0, 0, 0, 0.14);
  border-radius: 9999px;
  color: rgba(0, 0, 0, 0.45);
  font-size: 11px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  padding: 8px 16px;
}

[data-theme="light"] .filter-toggle.active {
  background: rgba(107, 100, 216, 0.10);
  border-color: rgba(107, 100, 216, 0.35);
  color: #6B64D8;
}
```

---

### Inputs (`.origin-input`)

```css
[data-theme="light"] .origin-input {
  background: rgba(255, 255, 255, 0.75);
  border: 0.5px solid rgba(0, 0, 0, 0.14);
  border-radius: 12px;
  color: #1a1a1a;
  font-family: Inter, sans-serif;
  font-size: 13px;
  padding: 12px 18px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

[data-theme="light"] .origin-input::placeholder {
  color: rgba(0, 0, 0, 0.28);
}

[data-theme="light"] .origin-input:focus {
  border-color: rgba(107, 100, 216, 0.40);
  box-shadow: 0 0 0 3px rgba(107, 100, 216, 0.08);
}
```

---

### Dropdowns (custom, not native `<select>`)

```css
[data-theme="light"] .origin-dropdown .glass-card {
  background: #FFFFFF;       /* Solid white — no blur needed for dropdowns */
  border: 0.5px solid rgba(0, 0, 0, 0.12);
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
}

[data-theme="light"] .dropdown-option {
  color: rgba(0, 0, 0, 0.55);
}

[data-theme="light"] .dropdown-option:hover {
  background: rgba(107, 100, 216, 0.07);
  color: #1a1a1a;
}
```

---

### Skill Chips / Badges

```css
[data-theme="light"] .jp-chip,
[data-theme="light"] .skill-badge {
  background: rgba(255, 255, 255, 0.55);
  border: 0.5px solid rgba(0, 0, 0, 0.14);
  border-radius: 9999px;
  color: rgba(0, 0, 0, 0.45);
  font-size: 10px;
  font-weight: 500;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  padding: 5px 13px;
}
```

---

### Sidebar Navigation

```css
[data-theme="light"] .sidebar-nav-item {
  color: rgba(0, 0, 0, 0.45);
}

[data-theme="light"] .sidebar-nav-item:hover {
  background: rgba(0, 0, 0, 0.04);
  color: #1a1a1a;
}

[data-theme="light"] .sidebar-nav-item.active {
  background: rgba(107, 100, 216, 0.09);
  color: #6B64D8;
}
```

---

## 5. Background Glows (Light Mode)

The glows must still be present but at a significantly lower opacity — they add warmth and depth without washing out the light background.

```css
[data-theme="light"] .glow-top-center {
  background: radial-gradient(
    ellipse,
    rgba(25, 95, 151, 0.10) 0%,
    transparent 70%
  );
}

[data-theme="light"] .glow-bottom-left {
  background: radial-gradient(
    ellipse,
    rgba(107, 100, 216, 0.08) 0%,
    transparent 70%
  );
}
```

> Reduce glow opacity from **15%** (dark) to **8–10%** (light). Any higher and the parchment background picks up a distracting color cast.

---

## 6. Charts in Light Mode

```js
// Chart.js light mode theme — apply globally
const chartThemLight = {
  backgroundColor: 'rgba(107, 100, 216, 0.65)',   // lm-purple at 65%
  borderColor: '#6B64D8',
  borderWidth: 1,
  borderRadius: 4,
};

// Axis tick color
scales: {
  y: {
    ticks: {
      color: 'rgba(0, 0, 0, 0.40)',
      font: { family: 'Inter', size: 11 }
    },
    grid: { display: false }
  },
  x: { display: false }
}
```

---

## 7. Theme Toggle Implementation

### Toggle Button (place in nav)

```html
<button
  id="theme-toggle"
  class="jp-nav-pill"
  onclick="toggleTheme()"
  aria-label="Toggle light/dark mode"
>
  ☀ LIGHT
</button>
```

### JavaScript

```js
function toggleTheme() {
  const root = document.documentElement;
  const isDark = root.getAttribute('data-theme') === 'dark';
  const next = isDark ? 'light' : 'dark';
  root.setAttribute('data-theme', next);
  localStorage.setItem('jp-theme', next);
  document.getElementById('theme-toggle').textContent =
    next === 'light' ? '☾ DARK' : '☀ LIGHT';
}

// On page load — respect saved preference
(function () {
  const saved = localStorage.getItem('jp-theme') ||
    (window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark');
  document.documentElement.setAttribute('data-theme', saved);
  const btn = document.getElementById('theme-toggle');
  if (btn) btn.textContent = saved === 'light' ? '☾ DARK' : '☀ LIGHT';
})();
```

### CSS Structure (how to wire both themes)

```css
/* === DARK (default) === */
:root,
:root[data-theme="dark"] {
  --bg:           #0f1011;
  --surface:      rgba(255, 255, 255, 0.03);
  --border:       rgba(255, 255, 255, 0.08);
  --text-primary: #ffffff;
  --text-secondary: rgba(255, 255, 255, 0.60);
  --text-tertiary:  rgba(255, 255, 255, 0.28);
  --accent-purple:  #847dff;
  --accent-blue:    #195f97;
  --accent-cyan:    #00b3dd;
}

/* === LIGHT === */
:root[data-theme="light"] {
  --bg:           #F4F1EC;
  --surface:      rgba(255, 255, 255, 0.60);
  --border:       rgba(0, 0, 0, 0.08);
  --text-primary: #1a1a1a;
  --text-secondary: rgba(0, 0, 0, 0.45);
  --text-tertiary:  rgba(0, 0, 0, 0.28);
  --accent-purple:  #6B64D8;
  --accent-blue:    #195f97;
  --accent-cyan:    #0099BB;
}

/* Then use --var everywhere instead of hardcoded colors */
.glass-card {
  background: var(--surface);
  border: 0.5px solid var(--border);
}

body {
  background: var(--bg);
  color: var(--text-primary);
}
```

---

## 8. What NOT to Do in Light Mode

| ❌ Wrong | ✅ Right |
|:---|:---|
| Pure white `#FFFFFF` page background | Warm parchment `#F4F1EC` |
| `rgba(255,255,255,0.03)` card surface | `rgba(255,255,255,0.60)` |
| White text on light cards | `#1a1a1a` primary text |
| Full-strength purple glow (15% opacity) | Reduced glow (8–10% opacity) |
| Native `<select>` dropdowns (white popup) | Custom dropdown with `background: #FFFFFF` + box-shadow |
| Keeping the dark CTA button style (`rgba(255,255,255,0.2)`) | Invert to dark pill on light background |

---

## 9. Light Mode Checklist

Before shipping light mode, verify:

- [ ] Page background is `#F4F1EC` (warm parchment), not `#FFFFFF`
- [ ] All cards use `rgba(255,255,255,0.60)` surface with `backdrop-filter: blur(20px)`
- [ ] Primary text is `#1a1a1a` — no white text visible anywhere in light mode
- [ ] CTA button is dark pill (`#1a1a1a` bg, parchment text)
- [ ] Active states use `lm-purple` (`#6B64D8`), not `#847dff` (too bright on white)
- [ ] Chart bars use `rgba(107,100,216,0.65)` fill
- [ ] Delta values use `lm-cyan` (`#0099BB`)
- [ ] Background glows are at 8–10% max opacity
- [ ] All dropdowns are custom (no native `<select>`) with white solid background
- [ ] Theme preference saved to `localStorage` and respected on page load
- [ ] `prefers-color-scheme: light` system preference is respected as default

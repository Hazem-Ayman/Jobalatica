# JobPulse — Design & Visual Identity Documentation

This document provides a comprehensive breakdown of the current visual identity and design system of JobPulse, redesigned in May 2026 based on the "Origin" high-fidelity aesthetic.

---

## 1. Design Philosophy: "The Professional Matrix"
The design is built to communicate **authority, innovation, and transparency**. It moves away from standard corporate "flat" design and adopts a sophisticated "AI Lab" aesthetic. 
- **Minimalism**: Removing unnecessary borders and using whitespace/padding to create breathing room.
- **High Fidelity**: Strategic use of transparency (glassmorphism) and lighting (radial glows).
- **Technical Depth**: Using monospaced-style tracking and uppercase labels to suggest precision.

---

## 2. Color Palette
The palette uses a deep, dark base with vibrant professional accents.

| Role | Color Name | Hex Code | Usage |
| :--- | :--- | :--- | :--- |
| **Background** | Origin Dark | `#0f1011` | Primary surface for all pages. |
| **Surface** | Glass Card | `rgba(255,255,255,0.03)` | Background for cards and containers. |
| **Primary Text** | Solid White | `#ffffff` | Headings and primary data. |
| **Secondary Text** | Muted White | `rgba(255,255,255,0.6)` | Descriptions and labels. |
| **Accent 1** | Origin Blue | `#195f97` | Primary brand identifier and charts. |
| **Accent 2** | Light Purple | `#847dff` | Interactive highlights and active states. |
| **Accent 3** | Cyan | `#00b3dd` | Success states and trend indicators. |

---

## 3. Typography System

### Headings (Serif)
- **Font**: Georgia (System fallback for Lyondisplay).
- **Weight**: 300 (Light).
- **Style**: `heading-massive`.
- **Characteristics**: Large font sizes (up to 100px), tight letter-spacing (`-0.04em`), often italicized for emphasis.
- **Usage**: Hero titles, section headers, large numeric stats.

### Body & UI (Sans-Serif)
- **Font**: Inter (Clean, geometric).
- **Weights**: 300 (Light) to 600 (Bold).
- **Characteristics**: High readability, uppercase for labels with high tracking (`0.3em`).
- **Usage**: Body copy, buttons, form labels, small metadata.

---

## 4. Key Components

### Glass Cards (`.glass-card`)
- **Background**: `rgba(255, 255, 255, 0.03)`.
- **Blur**: `backdrop-filter: blur(20px)`.
- **Border**: `1px solid rgba(255, 255, 255, 0.08)`.
- **Corner Radius**: `16px` (Standard) to `32px` (Section containers).

### Origin Buttons (`.origin-button`)
- **Shape**: Pill (Full rounded).
- **Background**: Semi-transparent `rgba(255, 255, 255, 0.2)` with blur.
- **Hover**: Transitions to `0.3` opacity with a subtle upward lift.
- **Iconography**: Always paired with an arrow (→) to indicate forward momentum.

### Origin Inputs (`.origin-input`)
- **Style**: Minimalist, no heavy borders.
- **State**: `focus` state glows with `origin-light-purple`.

---

## 5. Visual Elements & Motion

### Radial Glows (`.origin-bg-glow`)
Abstract "clouds" of color implemented via fixed radial gradients.
- Top center: Blue glow.
- Bottom left: Purple glow.
- Opacity: 15% to maintain subtlety.

### Data Visualization
- **Charts**: Custom Chart.js theme using `origin-light-purple` and `origin-blue` with transparent fills.
- **Animations**:
  - **Pulsing**: Small blue dots next to headlines to indicate "Live" data signals.
  - **Animated Counters**: Stats on the home page count up from 0 on load.
  - **Fade-in**: Hero text uses CSS transitions to appear smoothly.

---

## 6. UX Logic Implementation

### "Give-to-Get" Salary Model
A core design/business logic where advanced professional intelligence (Detailed salary tables, regional breakdowns) is visually "locked" behind a blurred overlay. 
- **Effect**: `blur-md opacity-20`.
- **Trigger**: Submission of a `SalaryReport` by the authenticated user.

### Live Matrix Search
- **HTMX Integration**: The search sidebar uses HTMX to update the `#job-results` div without page refreshes.
- **Feedback**: The search input triggers on `keyup delay:400ms`, providing a responsive, app-like feel.

---

## 7. Technical Implementation Layer
- **Tailwind CSS**: Used for 95% of styling via custom configuration in `_Layout.cshtml`.
- **Flowbite**: Powers the interactive Modals (Salary Popup) and Dropdowns.
- **HTMX**: Handles the "Engine" of the market marketplace.
- **Chart.js**: Visualizes demand and skill velocity.

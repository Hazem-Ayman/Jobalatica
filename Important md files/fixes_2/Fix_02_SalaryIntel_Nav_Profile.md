# Fix: Salary Intel — Remove from Navbar, Add to Profile
> Files: `Views/Shared/_Navbar.cshtml`, `Views/Profile/Index.cshtml`

---

## Problem

"Salary Intel" appears as a permanent navbar link visible to everyone at all times. This is wrong for two reasons:

1. It should only appear **once** as an onboarding prompt when the user is new (hasn't submitted salary data yet).
2. After that, it should live as a **section inside the Profile page** — not taking up permanent navbar space.

---

## Style Reference

> All markup below follows the JobPulse design language:
> - Background: `#E8E0D5` (warm cream)
> - Primary text: `#111111`
> - Muted/secondary text: `#888888`
> - Accent (coral): `#F25C5C`
> - Borders: `1px solid #C8C0B8`
> - Font: `'DM Sans', sans-serif` for UI; `'Playfair Display', Georgia, serif` for display headings
> - Labels: `font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase`
> - Buttons: flat, no border-radius beyond `2px`, black fill or outlined only — no shadows, no gradients

---

## Fix 1 — Remove "Salary Intel" from the Navbar Permanently

Open `Views/Shared/_Navbar.cshtml`.

Find the nav links section and **delete** the Salary Intel link entirely:

```html
<!-- DELETE this entire link -->
<a asp-controller="Salary" asp-action="Index">Salary Intel</a>
```

The navbar should only contain:
- Home
- Jobs
- Rankings
- Auth section (Log in / Get Started — or user avatar dropdown when logged in)

---

## Fix 2 — Show a One-Time Banner for New Users

In `Views/Home/Index.cshtml`, add a dismissible onboarding banner that appears only when the logged-in user has **never submitted salary data**.

Place this block immediately below the navbar, above all other page content:

```html
@* Show only if: user is logged in AND HasSubmittedSalary == false *@
@if (User.Identity.IsAuthenticated && !Model.HasSubmittedSalary)
{
    <div id="salary-onboarding-banner"
         style="background: #F25C5C; color: #E8E0D5; padding: 12px 40px; display: flex; align-items: center; justify-content: space-between; font-family: 'DM Sans', sans-serif;">

        <!-- Left: message -->
        <div style="display: flex; align-items: center; gap: 16px;">
            <!-- Small square marker — matches site's section label bullet -->
            <span style="display: inline-block; width: 8px; height: 8px; background: #E8E0D5; flex-shrink: 0;"></span>
            <span style="font-size: 13px;">
                <strong style="font-weight: 700;">Help the community:</strong>
                Share your salary anonymously and unlock full market intelligence.
            </span>
        </div>

        <!-- Right: CTA + dismiss -->
        <div style="display: flex; align-items: center; gap: 24px; flex-shrink: 0;">
            <a asp-controller="Salary" asp-action="Submit"
               style="font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #E8E0D5; text-decoration: none; border-bottom: 1px solid rgba(232,224,213,0.5);">
                SHARE NOW →
            </a>
            <button onclick="document.getElementById('salary-onboarding-banner').remove()"
                    style="background: none; border: none; color: #E8E0D5; font-size: 18px; line-height: 1; cursor: pointer; opacity: 0.7; padding: 0;">
                ×
            </button>
        </div>

    </div>
}
```

**In `HomeViewModel.cs`, add:**
```csharp
public bool HasSubmittedSalary { get; set; }
```

**In `HomeController.cs`, populate it:**
```csharp
var user = await _userManager.GetUserAsync(User);
Model.HasSubmittedSalary = user != null && await _context.SalarySubmissions
    .AnyAsync(s => s.UserId == user.Id);
```

---

## Fix 3 — Add Salary Section to Profile Page

Open `Views/Profile/Index.cshtml`.

Add the following block **below the Save Profile Changes button** and above the page footer. This becomes the permanent home for the Salary Intel feature.

```html
<!-- Salary Intel section — permanent on Profile page, replaces navbar link -->
<div style="border-top: 1px solid #C8C0B8; padding-top: 48px; margin-top: 48px;">

    <!-- Section label — matches site label pattern -->
    <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 12px;">
        <span style="display: inline-block; width: 10px; height: 10px; background: #F25C5C;"></span>
        <span style="font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888;">
            Salary Intelligence
        </span>
    </div>

    <!-- Card — bordered box matching site's "Live Signals" widget style -->
    <div style="border: 1px solid #C8C0B8; padding: 32px; display: flex; align-items: flex-start; justify-content: space-between; gap: 40px;">

        <!-- Left: description -->
        <div style="max-width: 420px;">
            <h3 style="font-family: 'Playfair Display', Georgia, serif; font-size: 22px; font-weight: 800; color: #111; margin: 0 0 10px 0;">
                Share Your Market Data
            </h3>
            <p style="font-size: 14px; color: #888; margin: 0; line-height: 1.6;">
                @if (Model.HasSubmittedSalary)
                {
                    <span>You've already contributed to the collective. Submit updated data at any time to keep your signal current.</span>
                }
                else
                {
                    <span>You haven't contributed yet. Share your salary anonymously to unlock full market insights and help the community.</span>
                }
            </p>
        </div>

        <!-- Right: CTA button — matches site's "ANALYZE →" and "VIEW DETAILS →" button style -->
        <div style="flex-shrink: 0; align-self: center;">
            <a asp-controller="Salary" asp-action="Submit"
               style="display: inline-block; background: #111; color: #E8E0D5; font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; padding: 14px 28px; text-decoration: none; border: 1px solid #111;">
                @(Model.HasSubmittedSalary ? "UPDATE SALARY DATA →" : "+ ADD NEW SALARY")
            </a>
        </div>

    </div>
</div>
```

**In `ProfileViewModel.cs`, add:**
```csharp
public bool HasSubmittedSalary { get; set; }
```

---

## Summary of Changes

| Location | Before | After |
|---|---|---|
| Navbar | Permanent "Salary Intel" link | Removed entirely |
| Home page (logged in, new user) | Nothing | Coral dismissible banner with `SHARE NOW →` CTA |
| Home page (logged in, returning user) | Nothing | Banner does not appear |
| Profile page | No salary section | Bordered card with heading, description, and CTA button |
| Button label | — | `UPDATE SALARY DATA →` if submitted · `+ ADD NEW SALARY` if not |
| Button style | Blue rounded pill | Black flat button with `1px solid #111`, site-standard uppercase label |
| Banner style | Orange rounded notification | Coral (`#F25C5C`) full-width bar with square bullet, all-caps CTA, `×` dismiss |

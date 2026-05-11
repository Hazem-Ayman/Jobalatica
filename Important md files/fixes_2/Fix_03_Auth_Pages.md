# Fix: Auth Pages — Login & Register
> Files: `Views/Account/Login.cshtml`, `Views/Account/Register.cshtml`, `Views/Shared/_Layout.cshtml`

---

## Problem

The Login and Register pages are visually inconsistent with the rest of the platform. They use a dark card, blue accent color, and rounded corners — none of which match the site's cream, serif, editorial design language.

---

## Style Reference

> All markup below follows the JobPulse design language:
> - Background: `#E8E0D5` (warm cream)
> - Primary text: `#111111`
> - Muted/secondary text: `#888888`
> - Accent (coral): `#F25C5C`
> - Borders: `1px solid #C8C0B8`
> - Inputs: `1px solid #C8C0B8`, background `#E8E0D5`, no border-radius
> - Font: `'DM Sans', sans-serif` for body; `'Playfair Display', Georgia, serif` for display headings
> - Labels: `font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888`
> - Buttons: flat, `background: #111`, `color: #E8E0D5`, no border-radius, no shadows, uppercase tracking label
> - No gradients. No drop shadows. No rounded corners.

---

## 1. Global Layout (`_Layout.cshtml`)

Ensure the site-wide layout has the correct base styles. Update the `<head>` to load the correct fonts:

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,700;0,800;1,700&family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet">
```

Ensure `<body>` has:
```html
<body style="background: #E8E0D5; font-family: 'DM Sans', sans-serif; color: #111; margin: 0;">
```

---

## 2. Auth Page Structure (Login & Register)

Both pages share the same outer layout. Replace your existing container and card markup with the structure below.

### Page Wrapper

```html
<div style="min-height: 100vh; background: #E8E0D5; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 64px 24px;">

    <!-- Brand mark — matches site navbar logo style -->
    <a href="/" style="display: flex; align-items: center; gap: 10px; margin-bottom: 56px; text-decoration: none;">
        <!-- Small black square — matches site's favicon/logo box -->
        <span style="display: inline-block; width: 20px; height: 20px; background: #111;"></span>
        <span style="font-family: 'DM Sans', sans-serif; font-size: 16px; font-weight: 700; color: #111; letter-spacing: -0.01em;">
            JobPulse
        </span>
    </a>

    <!-- Auth card — bordered box, no fill, matches site's widget border style -->
    <div style="width: 100%; max-width: 420px; border: 1px solid #C8C0B8; padding: 48px;">

        <!-- Section label above heading -->
        <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 16px;">
            <span style="display: inline-block; width: 8px; height: 8px; background: #F25C5C;"></span>
            <span style="font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; color: #888;">
                @(ViewData["Title"] == "Sign In" ? "Welcome Back" : "Get Started")
            </span>
        </div>

        <!-- Page heading — serif display, matches site H1 style -->
        <h1 style="font-family: 'Playfair Display', Georgia, serif; font-size: 36px; font-weight: 800; color: #111; margin: 0 0 8px 0; line-height: 1.05;">
            @(ViewData["Title"] == "Sign In" ? "Sign in to<br>your account." : "Create your<br>account.")
        </h1>
        <p style="font-size: 14px; color: #888; margin: 0 0 40px 0;">
            @(ViewData["Title"] == "Sign In"
                ? "Access your market data and intelligence."
                : "Start tracking your professional worth today.")
        </p>

        <!-- Form fields go here (see below) -->

    </div>

    <!-- Bottom switcher — below the card -->
    <p style="margin-top: 24px; font-size: 13px; color: #888; font-family: 'DM Sans', sans-serif;">
        @(ViewData["Title"] == "Sign In" ? "New here?" : "Already have an account?")
        <a href="@(ViewData["Title"] == "Sign In" ? "/Account/Register" : "/Account/Login")"
           style="color: #111; font-weight: 700; text-decoration: none; border-bottom: 1px solid #111; padding-bottom: 1px; margin-left: 4px;">
            @(ViewData["Title"] == "Sign In" ? "Create a free account" : "Sign in")
        </a>
    </p>

</div>
```

---

### Form Fields

Place these inside the card `<div>`, after the heading block.

```html
<div style="display: flex; flex-direction: column; gap: 24px;">

    <!-- Email field -->
    <div style="display: flex; flex-direction: column; gap: 6px;">
        <label style="font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888;">
            Email Address
        </label>
        <input type="email" placeholder="name@company.com"
               style="width: 100%; box-sizing: border-box; background: #E8E0D5; border: 1px solid #C8C0B8; color: #111; font-family: 'DM Sans', sans-serif; font-size: 14px; padding: 12px 14px; outline: none; transition: border-color 0.15s;"
               onfocus="this.style.borderColor='#111'" onblur="this.style.borderColor='#C8C0B8'"
               placeholder="name@company.com" />
    </div>

    <!-- Password field -->
    <div style="display: flex; flex-direction: column; gap: 6px;">
        <div style="display: flex; justify-content: space-between; align-items: center;">
            <label style="font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888;">
                Password
            </label>
            @if (ViewData["Title"] == "Sign In")
            {
                <a href="#" style="font-size: 12px; color: #888; text-decoration: none; border-bottom: 1px solid #C8C0B8;">
                    Forgot password?
                </a>
            }
        </div>
        <input id="password-field" type="password" placeholder="••••••••"
               style="width: 100%; box-sizing: border-box; background: #E8E0D5; border: 1px solid #C8C0B8; color: #111; font-family: 'DM Sans', sans-serif; font-size: 14px; padding: 12px 14px; outline: none; transition: border-color 0.15s;"
               onfocus="this.style.borderColor='#111'" onblur="this.style.borderColor='#C8C0B8'" />

        @* Password strength bar — Register page only *@
        @if (ViewData["Title"] == "Create Account")
        {
            <div style="margin-top: 8px;">
                <div style="width: 100%; height: 2px; background: #C8C0B8;">
                    <div id="strength-bar" style="height: 2px; width: 0%; background: transparent; transition: width 0.2s, background-color 0.2s;"></div>
                </div>
                <span id="strength-label" style="font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; margin-top: 4px; display: block; text-align: right; color: transparent;"></span>
            </div>
        }
    </div>

    @* Confirm password — Register only *@
    @if (ViewData["Title"] == "Create Account")
    {
        <div style="display: flex; flex-direction: column; gap: 6px;">
            <label style="font-size: 11px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; color: #888;">
                Confirm Password
            </label>
            <input type="password" placeholder="••••••••"
                   style="width: 100%; box-sizing: border-box; background: #E8E0D5; border: 1px solid #C8C0B8; color: #111; font-family: 'DM Sans', sans-serif; font-size: 14px; padding: 12px 14px; outline: none; transition: border-color 0.15s;"
                   onfocus="this.style.borderColor='#111'" onblur="this.style.borderColor='#C8C0B8'" />
        </div>
    }

    <!-- Submit button — matches site's "ANALYZE →" and "GET STARTED" button style -->
    <button type="submit"
            style="width: 100%; background: #111; color: #E8E0D5; font-family: 'DM Sans', sans-serif; font-size: 11px; font-weight: 700; letter-spacing: 0.15em; text-transform: uppercase; padding: 16px; border: 1px solid #111; cursor: pointer; margin-top: 8px; transition: background 0.15s;"
            onmouseover="this.style.background='#333'" onmouseout="this.style.background='#111'">
        @(ViewData["Title"] == "Sign In" ? "SIGN IN →" : "CREATE ACCOUNT →")
    </button>

</div>
```

---

## 3. Password Strength Script (Register Page Only)

Add this at the bottom of `Register.cshtml`. The strength bar uses the site's accent palette — coral for weak, moving toward black for strong — keeping the muted editorial tone.

```html
@section Scripts {
<script>
    document.getElementById('password-field').addEventListener('input', function () {
        const val = this.value;
        const bar = document.getElementById('strength-bar');
        const lbl = document.getElementById('strength-label');

        let width = '0%', color = 'transparent', label = '';

        if (val.length === 0) {
            // reset
        } else if (val.length < 4) {
            width = '25%'; color = '#F25C5C'; label = 'Too short';
        } else if (val.length < 8 || !/[A-Z]/.test(val) || !/[0-9]/.test(val)) {
            width = '50%'; color = '#C8905A'; label = 'Weak';
        } else if (val.length >= 8 && /[A-Z]/.test(val) && /[0-9]/.test(val)) {
            width = '75%'; color = '#888'; label = 'Good';
        }
        if (val.length >= 10 && /[A-Z]/.test(val) && /[0-9]/.test(val) && /[^A-Za-z0-9]/.test(val)) {
            width = '100%'; color = '#111'; label = 'Strong';
        }

        bar.style.width = width;
        bar.style.backgroundColor = color;
        lbl.style.color = color;
        lbl.textContent = label;
    });
</script>
}
```

---

## Summary of Changes

| Element | Before | After |
|---|---|---|
| Page background | Dark navy `#080E1A` | Cream `#E8E0D5` — matches entire site |
| Card background | Dark `#0F1829` with shadow | No fill — bordered box `1px solid #C8C0B8` |
| Card corners | `border-radius: 12px` | Square (`border-radius: 0`) |
| Heading font | `DM Sans` semibold | `Playfair Display` 800 — matches site H1 style |
| Section label | None | Coral square bullet + uppercase tracking label |
| Input style | Dark filled, blue focus ring | Cream background, black border on focus |
| Submit button | Blue rounded pill | Black flat button, uppercase tracking label, `→` suffix |
| Accent color | Blue `#2D7EF8` | Coral `#F25C5C` — matches site accent |
| Strength bar colors | Green/amber/red | Coral → amber → gray → black — editorial palette |
| Bottom switcher | Inside card | Below card, underline-only link style |
| Logo | SVG pulse icon + blue mono text | Black square + `JobPulse` in `DM Sans` — matches navbar |

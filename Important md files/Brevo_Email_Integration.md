# Brevo Email Integration — JobPulse
> Instructions for wiring up real email confirmation via Brevo SMTP into the ASP.NET Core Identity setup.

---

## Step 1 — Get Brevo SMTP Credentials (Manual — Human Must Do This)

> ⚠️ This step cannot be automated. The human must do this before you run anything.

1. Go to **https://app.brevo.com** and create a free account.
2. After logging in, go to: **Profile (top right) → SMTP & API → SMTP tab**.
3. Note down these four values — you will need them:
   - **SMTP Server:** `smtp-relay.brevo.com`
   - **Port:** `587`
   - **Login:** aaeadb001@smtp-brevo.com
   - **Password:** YOUR_BREVO_PASSWORD
4. Confirm your sender email under **Senders & IP → Senders → Add a Sender**.

---

## Step 2 — Install Required NuGet Package

Run this in the project root:

```bash
dotnet add package MailKit
```

> We use MailKit instead of the legacy `System.Net.Mail.SmtpClient` because it is fully async and is the modern .NET standard for SMTP.

---

## Step 3 — Add Brevo Settings to `appsettings.json`

Open `appsettings.json` and add the following block at the root level (alongside `ConnectionStrings`):

```json
"Brevo": {
  "SmtpHost": "smtp-relay.brevo.com",
  "SmtpPort": 587,
  "Login": "REPLACE_WITH_BREVO_LOGIN_EMAIL",
  "Password": "REPLACE_WITH_BREVO_SMTP_KEY",
  "SenderEmail": "noreply@jobpulse.com",
  "SenderName": "JobPulse"
}
```

> ⚠️ Replace the two `REPLACE_WITH_...` values with the real credentials from Step 1.
> Never commit real credentials to Git. Add `appsettings.json` to `.gitignore` or use User Secrets for production.

---

## Step 4 — Create the Email Sender Service

Create the file `Services/EmailSender.cs` with this exact content:

```csharp
using JobPulse.Models.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JobPulse.Services;

public class BrevoSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}

public class EmailSender : IEmailSender
{
    private readonly BrevoSettings _settings;

    public EmailSender(IOptions<BrevoSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Login, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```

---

## Step 5 — Register the Service in `Program.cs`

Add these two lines inside `Program.cs`, **before** `builder.Build()`:

```csharp
// Brevo email sender
builder.Services.Configure<BrevoSettings>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
```

Make sure this using is at the top of `Program.cs`:

```csharp
using Microsoft.AspNetCore.Identity.UI.Services;
using JobPulse.Services;
```

---

## Step 6 — Enable Email Confirmation in Identity Options

In `Program.cs`, find the `AddDefaultIdentity` call and make sure it looks like this:

```csharp
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;   // ← this line is critical
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
```

---

## Step 7 — Send Confirmation Email in the Register Action

In `AccountController.cs` (or wherever your Register POST action lives), after successfully creating the user with `_userManager.CreateAsync`, add this block:

```csharp
// Generate confirmation token
var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

// Build confirmation link
var confirmLink = Url.Action(
    "ConfirmEmail",
    "Account",
    new { userId = user.Id, token = encodedToken },
    Request.Scheme
);

// Send email
await _emailSender.SendEmailAsync(
    user.Email!,
    "Confirm your JobPulse account",
    $@"
    <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:32px;'>
        <h2 style='color:#FF4D00;'>Welcome to JobPulse 👋</h2>
        <p>Thanks for signing up. Click the button below to confirm your email address.</p>
        <a href='{confirmLink}'
           style='display:inline-block;margin-top:16px;padding:12px 28px;
                  background:#FF4D00;color:#fff;border-radius:8px;
                  text-decoration:none;font-weight:600;'>
            Confirm My Account
        </a>
        <p style='margin-top:24px;color:#888;font-size:13px;'>
            If you didn't sign up for JobPulse, you can safely ignore this email.
        </p>
    </div>
    "
);

TempData["Info"] = "Registration successful! Please check your email to confirm your account.";
return RedirectToAction("Login");
```

Add these usings at the top of the controller file:

```csharp
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
```

---

## Step 8 — Create the `ConfirmEmail` Action

In `AccountController.cs`, add this action that handles the link the user clicks in their email:

```csharp
[HttpGet]
public async Task<IActionResult> ConfirmEmail(string userId, string token)
{
    if (userId == null || token == null)
        return RedirectToAction("Index", "Home");

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        return NotFound();

    var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
    var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

    if (result.Succeeded)
    {
        TempData["Success"] = "Email confirmed! You can now log in.";
        return RedirectToAction("Login");
    }

    TempData["Error"] = "Email confirmation failed. The link may have expired.";
    return RedirectToAction("Login");
}
```

---

## Step 9 — Verify It Works

Run the project and:

1. Register a new user with a **real email address** you can access.
2. Check that the app redirects to Login with a "check your email" message.
3. Open your inbox — a JobPulse confirmation email should arrive within seconds.
4. Click the confirmation link.
5. Open **DB Browser for SQLite** → `AspNetUsers` table → confirm that `EmailConfirmed` column changed from `0` to `1` for that user.
6. Log in — it should succeed now.

---

## Full Flow Summary

```
User submits Register form
        ↓
Identity creates user in SQLite (EmailConfirmed = 0)
        ↓
Controller generates token → builds confirm URL
        ↓
EmailSender.SendEmailAsync() → MailKit → Brevo SMTP → real inbox
        ↓
User clicks link in email → ConfirmEmail action
        ↓
Identity sets EmailConfirmed = 1 in SQLite
        ↓
User can now log in successfully
```

---

## Files Modified / Created Summary

| File | Action |
|---|---|
| `appsettings.json` | Add `Brevo` config section |
| `Services/EmailSender.cs` | Create (new file) |
| `Program.cs` | Register `BrevoSettings` + `EmailSender`, set `RequireConfirmedAccount = true` |
| `Controllers/AccountController.cs` | Add email sending to Register POST + add ConfirmEmail GET action |

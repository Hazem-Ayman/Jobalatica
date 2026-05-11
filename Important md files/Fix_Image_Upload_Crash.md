# Fix Image Upload Crash — Jobalatica
> The app exits with code -1 (0xffffffff) every time a user uploads a profile picture.
> This document tells you exactly what to do to find and fix the crash.

---

## Step 1 — Add Global Exception Logging

Open `Program.cs` and wrap the entire content in a try-catch so the real error is printed before the app dies.

Find the **very bottom** of `Program.cs` where `app.Run()` is called and replace it with:

```csharp
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== FATAL CRASH ===");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("===================");
    File.WriteAllText("crash.log", ex.ToString());
}
```

This writes the full exception to both the console AND a `crash.log` file in the project root.

---

## Step 2 — Add Try-Catch Around the Upload Logic

Find the controller action that handles profile picture saving (likely `ProfileController.cs`, action `SaveProfile` or similar).

Wrap the **entire action body** in a try-catch like this:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[RequestSizeLimit(5242880)] // 5MB max
public async Task<IActionResult> SaveProfile(ProfileViewModel model)
{
    try
    {
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            // Validate extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProfilePicture", "Only .jpg, .jpeg, .png, .webp files are allowed.");
                return View(model);
            }

            // Validate size (2MB)
            if (model.ProfilePicture.Length > 2097152)
            {
                ModelState.AddModelError("ProfilePicture", "Image must be under 2MB.");
                return View(model);
            }

            // Make sure upload folder exists
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadFolder); // safe even if folder already exists

            // Save file
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePicture.CopyToAsync(stream);
            }

            // Save path to user in DB
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            user.ProfilePictureUrl = "/uploads/profiles/" + fileName;
            await _userManager.UpdateAsync(user);
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        // Log to file so we can read it
        File.AppendAllText("crash.log",
            $"\n[{DateTime.Now}] Upload crash:\n{ex}\n");

        ModelState.AddModelError("", "Upload failed: " + ex.Message);
        return View(model);
    }
}
```

---

## Step 3 — Fix the Upload Folder (Most Likely Root Cause)

Run these commands to create the upload folder and a placeholder file so it's included in the build:

```bash
mkdir -p wwwroot/uploads/profiles
echo "" > wwwroot/uploads/profiles/.gitkeep
```

---

## Step 4 — Fix Request Size Limits

Add this to `Program.cs` **before** `builder.Build()`:

```csharp
// Allow file uploads up to 5MB
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5242880; // 5MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5242880; // 5MB
});
```

---

## Step 5 — Fix the Navbar Null Reference

Open `Views/Shared/_Navbar.cshtml` and find where the profile picture is displayed.

Replace any direct image reference with a null-safe check:

```html
@{
    var picUrl = ViewBag.ProfilePictureUrl as string;
}

@if (!string.IsNullOrEmpty(picUrl))
{
    <img src="@picUrl"
         class="w-9 h-9 rounded-full object-cover"
         alt="Profile" />
}
else
{
    <span class="w-9 h-9 rounded-full bg-[#FF4D00] flex items-center justify-center text-white text-sm font-bold">
        @* Initials fallback *@
        JD
    </span>
}
```

---

## Step 6 — Make Sure IWebHostEnvironment Is Injected

Open `ProfileController.cs` and confirm the constructor looks like this:

```csharp
private readonly UserManager<ApplicationUser> _userManager;
private readonly IWebHostEnvironment _environment;

public ProfileController(
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment)
{
    _userManager = userManager;
    _environment = environment;
}
```

If `_environment` is missing from the constructor, `_environment.WebRootPath` throws a null reference crash — which is a common cause of this exact exit code.

---

## Step 7 — Read the Crash Log

After applying all fixes, run the app and try uploading again.

If it still crashes, read the log file to get the exact error:

```bash
cat crash.log
```

The full exception stack trace will be there. Report it for further debugging.

---

## Checklist — Apply in Order

| # | Fix | File |
|---|---|---|
| 1 | Wrap `app.Run()` in try-catch + write `crash.log` | `Program.cs` |
| 2 | Wrap upload action in try-catch + validate file | `ProfileController.cs` |
| 3 | Create `wwwroot/uploads/profiles/` folder with `.gitkeep` | File system |
| 4 | Add `FormOptions` + `Kestrel` size limits | `Program.cs` |
| 5 | Add null check for profile picture in navbar | `_Navbar.cshtml` |
| 6 | Confirm `IWebHostEnvironment` is injected in constructor | `ProfileController.cs` |
| 7 | Run app → upload image → if still crashes, read `crash.log` | — |

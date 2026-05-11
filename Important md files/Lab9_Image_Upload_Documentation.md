# Image Upload in ASP.NET Core MVC — Course Documentation
> Extracted from Lab 9 (Asp_Net_MVC_Core_9.pptx)
> Professor: Mostafa Abobakr Salem | Eng. Mohamed Galal | Eng. Mena Nagy | Eng. Omnia Ahmed

---

## Overview

The course teaches image upload through the `Department` entity in the "FCI Program" sample app. A department has a `department_Pic` property that stores the image filename. The image file itself is saved to `wwwroot/Img/`.

---

## Step 1 — Add Image Property to the Model

In your entity model, add a string property to store the image filename:

```csharp
[Display(Name = "Image")]
[DefaultValue("default.png")]
public string department_Pic { get; set; }
```

> The property stores only the **filename** (e.g. `photo.png`), not the full path.

---

## Step 2 — Update the Create View

Open `Views/Department/Create.cshtml` and make two changes:

**1. Add `enctype="multipart/form-data"` to the form tag** — this is required for any form that uploads files:

```html
<form asp-action="Create" enctype="multipart/form-data">
```

**2. Change the image input to `type="file"`** and give it the name `img_file`:

```html
<div class="form-group">
    <label asp-for="department_Pic" class="control-label"></label>
    <input asp-for="department_Pic" class="form-control" type="file" name="img_file" />
    <span asp-validation-for="department_Pic" class="text-danger"></span>
</div>
```

**3. Optionally show an upload success message** from ViewBag:

```html
@if (ViewBag.Message != null) {
    <span style="color:orangered">@Html.Raw(ViewBag.Message)</span>
}
```

---

## Step 3 — Update the Create Action (Controller)

Open `Controllers/DepartmentController.cs`.

### Part A — Save the file to `wwwroot/Img/`

Add `IFormFile img_file` as a parameter. The action must be `async`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    [Bind("dept_Name")] Department department,
    IFormFile img_file)
{
    // Build the path to wwwroot/Img/
    string path = Path.Combine(_environment.WebRootPath, "Img");

    // Create the folder if it doesn't exist
    if (!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
    }

    if (img_file != null) {
        path = Path.Combine(path, img_file.FileName); // e.g. wwwroot/Img/photo.png
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await img_file.CopyToAsync(stream);
            ViewBag.Message = string.Format(
                "<b>{0}</b> uploaded.<br/>",
                img_file.FileName.ToString()
            );
        }
    }

    return View();
}
```

### Part B — Also save the filename to the database

Extend the same action to persist the filename and save to DB:

```csharp
if (img_file != null) {
    path = Path.Combine(path, img_file.FileName);
    using (var stream = new FileStream(path, FileMode.Create))
    {
        await img_file.CopyToAsync(stream);
        ViewBag.Message = string.Format(
            "<b>{0}</b> uploaded.<br/>",
            img_file.FileName.ToString()
        );
    }
    department.department_Pic = img_file.FileName; // ← save filename to DB
}
else {
    department.department_Pic = "default.jpeg"; // ← fallback if no image uploaded
}

try {
    _context.Add(department);
    _context.SaveChanges();
    return RedirectToAction("Index");
}
catch (Exception ex) {
    ViewBag.exc = ex.Message;
}

return View(department);
```

> **Key points:**
> - `_environment` is `IWebHostEnvironment` — inject it via constructor.
> - `img_file.FileName` is saved to the database, not the full path.
> - Always provide a default image fallback when no file is uploaded.

---

## Step 4 — Display the Image in the Index View

Open `Views/Department/Index.cshtml` and reference the saved filename inside `~/Img/`:

```html
@foreach (var item in Model) {
    <tr>
        <td>
            @Html.DisplayFor(modelItem => item.dept_Name)
        </td>
        <td>
            <a asp-action="Details" asp-route-id="@item.Code">
                <img src="~/Img/@item.department_Pic"
                     alt="Department Image"
                     style="border-radius:50%; width:70px; height:70px" />
            </a>
        </td>
    </tr>
}
```

---

## Step 5 — Display the Image in the Details View

Open `Views/Department/Details.cshtml`:

```html
<img src="~/Img/@Model.department_Pic"
     style="width:150px; border-radius:50%"
     alt="Department Image" />
```

---

## Summary — How It All Connects

```
User picks a file in the form (type="file", name="img_file")
        ↓
Form submits with enctype="multipart/form-data"
        ↓
Controller receives IFormFile img_file
        ↓
File is saved to  wwwroot/Img/filename.png  on the server
        ↓
filename.png  is saved as a string in the database column
        ↓
Views display the image using  ~/Img/@Model.department_Pic
```

---

## Required: Inject IWebHostEnvironment

To use `_environment.WebRootPath` in your controller, inject it via the constructor:

```csharp
private readonly ApplicationDBcontext _context;
private readonly IWebHostEnvironment _environment;

public DepartmentController(ApplicationDBcontext context, IWebHostEnvironment environment)
{
    _context = context;
    _environment = environment;
}
```

---

## Full File & Folder Structure

```
wwwroot/
└── Img/
    ├── default.jpeg       ← fallback image (add this manually)
    └── [uploaded files]   ← created automatically on upload

Models/
└── Department.cs          ← department_Pic string property

Views/Department/
├── Create.cshtml          ← enctype + type="file" input
├── Index.cshtml           ← <img src="~/Img/@item.department_Pic">
└── Details.cshtml         ← <img src="~/Img/@Model.department_Pic">

Controllers/
└── DepartmentController.cs ← async Create POST with IFormFile
```

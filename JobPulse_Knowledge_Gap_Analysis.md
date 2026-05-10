# JobPulse — Knowledge Gap Analysis

> Can you build this project with only your course knowledge?
> **Short answer: No, not fully.** Your course covers roughly 60% of what JobPulse needs. The core MVC, EF Core, and views are well covered — but 4 critical topics are completely absent from your slides and are required by the project.

---

## ✅ Covered in the Course

### MVC Pattern — Controllers, Views, Routing
Labs 2–4 covered controllers, action methods, HttpGet/HttpPost, URL parameters, ViewData, ViewBag, strongly-typed views, `_Layout.cshtml`, partial views, and RenderBody. This maps directly to your Controllers/ and Views/ folders.

### Entity Framework Core — Code First
Labs 5–7 covered entity classes, DbContext, DbSet, migrations (`add-migration` / `update-database`), connection strings in `appsettings.json`, and basic LINQ queries (`ToList`, `Add`, `SaveChanges`).

### Data Annotations
Lab 8 covered `[Key]`, `[ForeignKey]`, `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`, `[Display]`, `[Column]` — all needed for your entity classes and ViewModels.

### Relationships + Navigation Properties
Labs 6 & 9 covered one-to-many relationships, `ICollection<T>`, virtual navigation properties, and ForeignKey wiring — you need this for `Jobs → JobSkills → Skills` and `User → SavedJobs`.

### File Upload Handling
Lab 9 covered `IFormFile`, saving files to `wwwroot`, and `ValidateAntiForgeryToken`. Not directly needed but demonstrates the async action patterns you'll use throughout the project.

### Scaffolding CRUD Controllers
Lab 9 showed auto-generating controllers with views using EF scaffolding — useful reference even though JobPulse uses manually written controllers.

---

## 🔴 Not in the Course — Must Learn

### 1. ASP.NET Core Identity
Zero coverage in any slide. Yet the project needs full user registration, login, logout, `[Authorize]`, `UserManager`, `SignInManager`, `IdentityUser` extension (`ApplicationUser`), and `IdentityDbContext`. This is Phases 3 and 6 of your build — a substantial chunk of the project.

**Where to learn:** Microsoft Docs — *"Introduction to Identity on ASP.NET Core"*

### 2. Dependency Injection + Service Layer (Program.cs)
The course shows `AddDbContext` but never teaches `AddScoped`, interface-based services (`IJobService` / `JobService`), or constructor injection into controllers. Your entire `Services/` layer depends on this pattern.

**Where to learn:** Microsoft Docs — *"Dependency injection in ASP.NET Core"*

### 3. Advanced LINQ — GroupBy, Include, Skip/Take
The course only shows basic `.ToList()` and `.Add()`. JobPulse needs:
- `.Include()` for eager loading (e.g. loading a Job with its Skills)
- `.GroupBy().OrderByDescending()` for the rankings page
- `.Skip().Take()` for search result pagination

None of these appear in any slide.

**Where to learn:** Microsoft Docs — *"Querying Data — EF Core"*

### 4. ViewModels Pattern
The course passes raw entity objects directly to views. The spec strictly forbids this and requires dedicated ViewModel classes (`JobSearchViewModel`, `HomeViewModel`, etc.). The pattern of never passing entities to views is not taught anywhere in the slides.

**Where to learn:** Any ASP.NET Core MVC tutorial that covers the ViewModel pattern (e.g. the official "MVC Movie" tutorial on Microsoft Docs).

---

## 🟡 Partially Covered — Needs Extension

### Fluent API in OnModelCreating
The course shows `[Key]` and `[ForeignKey]` annotations but never teaches `modelBuilder.Entity().HasIndex()`, `.HasPrecision()`, or `.ToTable()` — all required in your `ApplicationDbContext`.

### Async/Await in Controllers
Lab 9 shows one async action for file upload, but the project requires async throughout (`SearchAsync`, `GetByIdAsync`, `SaveChangesAsync`). You need to understand `Task<IActionResult>` and awaiting service calls.

### TempData
TempData is mentioned by name in Lab 4 but never demonstrated with an actual usage example. The project uses it for success/error flash messages after POST → redirect flows.

### Frontend Tools: HTMX, Chart.js, Tailwind, Flowbite
Not ASP.NET topics, but none appear in the course. They are added via CDN so the barrier is low — but you need to learn them separately. HTMX live search and Chart.js bar charts are required features.

---

## 📋 Study Priority Order

Start building immediately with what you know (Phases 1–2), and learn the missing topics in this order:

| Priority | Topic | Needed by |
|---|---|---|
| ① | ASP.NET Core Identity | Phase 3 / Phase 6 |
| ② | Dependency injection & services | Phase 2 (service layer) |
| ③ | Advanced LINQ (Include, GroupBy, pagination) | Phase 2 (rankings + search) |
| ④ | ViewModel pattern | Phase 3 (views) |
| ⑤ | HTMX + Chart.js basics | Phase 5 (polish) |

> **Good news:** Phases 1 and 2 (foundation + job search/rankings) you can start right now with what you already know. You only hit the Identity wall at Phase 3 — so start building and learn Identity in parallel.

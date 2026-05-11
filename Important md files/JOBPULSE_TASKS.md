# JobPulse — Solo Developer Task List

> Layout guide: tasks stacked vertically = do in order (each depends on the one above).
> Tasks side by side in the same block = can be done in any order or at the same time.

---

## Phase 1 — Project Foundation

- [x] **Create the ASP.NET MVC Core project**
  - Run `dotnet new mvc -n JobPulse`
  - Verify it builds and runs a blank page before touching anything else
  - This is the root — every file lives inside it
  - Done: MVC project exists as `Jobalatica/Jobalatica.csproj` and builds successfully.
  - Knowledge gap: No. The gap analysis says MVC controllers/views/routing are covered by the course.

- [x] **Install all NuGet packages**
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - Nothing else works without these three
  - Done: All three packages are referenced in `Jobalatica.csproj`; build verified.
  - Knowledge gap: Partial. EF Core is covered, but ASP.NET Core Identity is listed as a missing course topic.

- [x] **Write all entity classes** → `Models/Entities/*.cs`
  - 8 files, one per DB table, columns must match the schema exactly
  - `Job.cs` — maps to Jobs table (pipeline data, read-only)
  - `Skill.cs` — maps to Skills table (pipeline data, read-only)
  - `JobSkill.cs` — junction table Jobs ↔ Skills (pipeline data, read-only)
  - `ApplicationUser.cs` — extends IdentityUser, adds DisplayName + ExperienceLevel + CreatedAt + LastLoginAt
  - `UserSkill.cs` — junction table Users ↔ Skills (web app owned)
  - `DemandSnapshot.cs` — weekly demand snapshots per role/location (pipeline data, read-only)
  - `SalaryReport.cs` — user-submitted salary data (web app owned)
  - `SavedJob.cs` — jobs bookmarked by users (web app owned)
  - Get these right before running migrations — wrong entities = broken schema
  - Done: All 8 entity files exist under `Models/Entities` and match the project spec.
  - Knowledge gap: Partial. Entity classes and relationships are covered, but `ApplicationUser : IdentityUser` depends on ASP.NET Core Identity, which is listed as a gap.

- [x] **Write ApplicationDbContext** → `Data/ApplicationDbContext.cs`
  - Extends `IdentityDbContext<ApplicationUser>`
  - Register all DbSets (Jobs, Skills, JobSkills, UserSkills, DemandSnapshots, SalaryReports, SavedJobs)
  - Configure in `OnModelCreating`:
    - Unique index on SavedJob(UserId + JobId)
    - Unique index on UserSkill(UserId + SkillId)
    - Decimal precision (10,2) on all salary columns
    - Map pipeline tables to their exact table names with `.ToTable()`
  - This is the bridge between C# code and the database — everything reads/writes through here
  - Done: `ApplicationDbContext` was added with Identity inheritance, DbSets, table mappings, unique indexes, and salary decimal precision.
  - Knowledge gap: Partial. DbContext/DbSet are covered, but `IdentityDbContext` is an Identity gap and Fluent API configuration is listed as partially covered.

- [x] **Configure Program.cs** → `Program.cs`
  - `AddDbContext` with SQL Server connection string
  - `AddIdentity<ApplicationUser, IdentityRole>` → `AddEntityFrameworkStores<ApplicationDbContext>`
  - `AddScoped` for each service interface (IJobService, IRankingService, IRecommendationService, ISalaryService)
  - Middleware order (MUST be exactly this): `UseStaticFiles` → `UseRouting` → `UseAuthentication` → `UseAuthorization`
  - Wrong middleware order causes silent auth bugs
  - Done: `Program.cs` now registers `ApplicationDbContext`, ASP.NET Core Identity, all service interfaces, and uses the required auth middleware order.
  - Knowledge gap: Yes. The gap analysis lists ASP.NET Core Identity and dependency injection/service registration as missing course topics.

- [x] **Set connection string** → `appsettings.json`
  - Add `ConnectionStrings.DefaultConnection` pointing to local SQL Server
  - Use `TrustServerCertificate=True` for dev
  - Never hardcode the connection string anywhere in C# files
  - Done: `DefaultConnection` now points to local SQLite file `JobPulse.db` for temporary DB Browser workflow, and `Program.cs` uses `UseSqlite`.
  - Knowledge gap: No for the connection string itself. The gap analysis says EF Core connection strings in `appsettings.json` are covered; SQLite provider usage is a small provider-specific adjustment outside the original SQL Server plan.

- [x] **Run first EF migration and update database**
  - `dotnet ef migrations add InitialCreate`
  - `dotnet ef database update`
  - Open SSMS or Azure Data Studio and verify every table exists with correct columns
  - If anything is wrong: fix the entity → delete the migration folder → re-run both commands
  - Files auto-generated in `Data/Migrations/` — never edit manually
  - Done: Created `InitialCreate` migration under `Data/Migrations` and applied it to SQLite database `JobPulse.db`.
  - Knowledge gap: No for EF migrations. The gap analysis says EF Core Code First migrations and database updates are covered by the course; using SQLite is a temporary provider change.

- [x] **Write mock data seeder** → `Data/DbSeeder.cs`
  - Static class `DbSeeder` with `SeedAsync(ApplicationDbContext db)` method
  - Call it from `Program.cs` on startup only if the DB is empty
  - Seed:
    - 25 canonical Skills (Python, JavaScript, React, SQL, Docker, AWS, TypeScript, Node.js, Kubernetes, Git, .NET, Java, PostgreSQL, GraphQL, Flutter, C#, Angular, Vue, Redis, MongoDB, Azure, Kotlin, Swift, Go, PHP)
    - 100 Jobs across 5 cities (Cairo, Alexandria, Dubai, Riyadh, Remote) and 15 roles
    - JobSkills linking each job to 3–6 skills
    - 20 DemandSnapshot rows (4 roles × 5 weekly snapshots each)
  - Without this, every page is blank and nothing can be tested
  - Done: `DbSeeder.SeedAsync` seeds 25 skills, 100 jobs, 444 job-skill links, and 20 demand snapshots. `Program.cs` calls it on startup and supports `--seed-only` for verification.
  - Knowledge gap: Partial. EF `Add`/`SaveChanges` patterns are covered, but async seeding and startup integration use partially covered async concepts.

> **Checkpoint:** Done. A temporary `Home/SeedCheck` action returned 5 seeded job rows from `db.Jobs.Take(5).ToList()` as JSON, then the action was removed.

---

## Phase 2 — Service Layer

> These 4 service pairs have no dependency on each other — write them in any order.

---

| | |
|---|---|
| **IJobService + JobService** → `Services/JobService.cs + IJobService.cs` | **IRankingService + RankingService** → `Services/RankingService.cs + IRankingService.cs` |
| `SearchAsync(query, location, experienceLevel, salaryMin, salaryMax, skillIds, page, pageSize)` — returns `(List<Job> Jobs, int TotalCount)`. Build as a composable LINQ query: start with `db.Jobs.Where(j => j.IsActive)`, chain each optional filter with `if (param != null)`, then `.Skip().Take()` for paging. | `GetTopRolesAsync(count)` — GROUP BY Title, ORDER BY COUNT DESC. `GetTopSkillsAsync(count)` — ORDER BY TotalJobMentions DESC. `GetRoleTrendAsync(title, location)` — DemandSnapshots for a title ordered by date. `GetSalaryRangeAsync(title, location)` — average Jobs.SalaryMin/Max + SalaryReports.Salary blended together. |
| `GetByIdAsync(id)` — single job with JobSkills and Skills eagerly loaded via `.Include()`. | Powers the entire rankings page and role detail page. |
| `GetRecentAsync(count)` — newest N active jobs by PostedAt DESC. | |
| `IsJobSavedAsync(userId, jobId)` — returns bool. | |
| `SaveJobAsync(userId, jobId)` — insert SavedJob row (catch duplicate key exception gracefully). | |
| `UnsaveJobAsync(userId, jobId)` — delete SavedJob row. | |

Done: `IJobService`, `JobService`, `IRankingService`, and `RankingService` were added with EF Core LINQ implementations and registered in DI.
Knowledge gap: Yes. The gap analysis lists dependency injection/service layer and advanced LINQ (`Include`, `GroupBy`, `Skip/Take`) as missing course topics.

---

| | |
|---|---|
| **IRecommendationService + RecommendationService** → `Services/RecommendationService.cs + IRecommendationService.cs` | **ISalaryService + SalaryService** → `Services/SalaryService.cs + ISalaryService.cs` |
| `GetRecommendedJobsAsync(userId, count)` — get user's SkillIds from UserSkills table → query Jobs where any JobSkill.SkillId is in that set → group by JobId and count overlapping skills → order by overlap count DESC → return top N. Pure LINQ, no ML needed. Powers the personalized feed on the home page. | `SubmitReportAsync(SalaryReport report)` — validate and insert into SalaryReports table. Set `SubmittedAt = DateTime.UtcNow` before saving. |
| | `GetReportsForRoleAsync(jobTitle)` — return all SalaryReports where JobTitle matches (case-insensitive). Used by RankingService to blend community data into salary averages. |

Done: `IRecommendationService`, `RecommendationService`, `ISalaryService`, and `SalaryService` were added with EF Core implementations and registered in DI.
Knowledge gap: Yes. The gap analysis lists dependency injection/service layer and advanced LINQ as missing course topics; async usage is partially covered.

---

## Phase 3 — ViewModels

> Write all ViewModels before writing any controller or view. They define the data contract between the two.

---

| | | |
|---|---|---|
| [x] **HomeViewModel** → `Models/ViewModels/HomeViewModel.cs` | [x] **JobSearchViewModel** → `Models/ViewModels/JobSearchViewModel.cs` | [x] **JobDetailViewModel** → `Models/ViewModels/JobDetailViewModel.cs` |
| `List<(string Title, int Count)> TopRoles` | `List<Job> Jobs` | `Job Job` |
| `List<Skill> TopSkills` | `int TotalCount` | `bool IsSaved` — toggled by the save button |
| `List<Job> RecentJobs` | `int CurrentPage` | `List<Job> SimilarJobs` |
| `List<Job>? PersonalizedJobs` — null when user is logged out. Null = show locked teaser section. Non-null = show personalized feed. | `int PageSize = 20` | |
| | `int TotalPages` — computed: `(int)Math.Ceiling((double)TotalCount / PageSize)` | |
| | Mirror all filter fields: `Query`, `Location`, `ExperienceLevel`, `SalaryMin`, `SalaryMax` — needed to repopulate the sidebar form after a search so it doesn't reset | |

---

| | | |
|---|---|---|
| [x] **RankingsViewModel** → `Models/ViewModels/RankingsViewModel.cs` | [x] **RoleDetailViewModel** → `Models/ViewModels/RoleDetailViewModel.cs` | [x] **ProfileViewModel + SalarySubmitViewModel** → `Models/ViewModels/ProfileViewModel.cs` + `SalarySubmitViewModel.cs` |
| `List<(string Title, int Count)> TopRoles` | `string JobTitle` | ProfileViewModel: `string DisplayName`, `string ExperienceLevel`, `List<Skill> AllSkills`, `List<int> UserSkillIds`, `List<SavedJob> SavedJobs` |
| `List<Skill> TopSkills` | `decimal AvgSalaryMin` | SalarySubmitViewModel: form fields with DataAnnotations — `[Required] string JobTitle`, `[Required] string Location`, `[Range(0, 99999)] decimal Salary`, `[Required] string Currency`, `[Range(0, 40)] int YearsExperience`, `string SkillsList` |
| | `decimal AvgSalaryMax` | |
| | `int SampleSize` — number of data points behind the salary figures | |
| | `List<DemandSnapshot> Trend` — ordered by SnapshotDate ASC for the line chart | |
| | `List<Job> SampleJobs` | |
| | `List<Skill> CommonSkills` | |

---

## Phase 4 — Layout and Shared Views

> Build these once. Every page inherits from them.

- [x] **_Layout.cshtml** → `Views/Shared/_Layout.cshtml`
  - The shell every page inherits via `@{ Layout = "_Layout"; }`
  - In `<head>`: Tailwind CDN (`<script src="https://cdn.tailwindcss.com">`), Flowbite CSS CDN
  - In `<body>`: `@await Html.PartialAsync("_Navbar")`, `<main>@RenderBody()</main>`
  - Before `</body>`: HTMX CDN, Flowbite JS CDN, Chart.js CDN, `@await RenderSectionAsync("Scripts", required: false)`
  - The Scripts section is critical — individual views inject their Chart.js code here
  - Getting this file right means every page automatically has all libraries

- [x] **_Navbar.cshtml** → `Views/Shared/_Navbar.cshtml` | [x] **_JobCard.cshtml** → `Views/Shared/_JobCard.cshtml`

  | _Navbar.cshtml | _JobCard.cshtml |
  |---|---|
  | [x] Rendered inside `_Layout.cshtml` via `@await Html.PartialAsync("_Navbar")` | [x] `@model Job` — takes a Job entity, renders a single card |
  | Left: logo/wordmark | Left 3px accent bar (color encodes demand) |
  | Center: nav links (Home / Jobs / Rankings) | Job title, company, location, salary range (monospace) |
  | Right: conditional on `@User.Identity?.IsAuthenticated` — logged out = "Sign In" + "Get Started" buttons, logged in = avatar + dropdown (Profile, Saved Jobs, Sign Out) | Up to 4 skill pills + source badge + posted date + demand badge |
  | Active link highlighted using `ViewContext.RouteData.Values["controller"]` comparison | Save bookmark icon + "View Details →" button |
  | Mobile: Flowbite navbar collapse with hamburger | Used on home, search results, and role detail pages — write once, reuse everywhere via `@await Html.PartialAsync("_JobCard", job)` |

---

## Phase 5 — Controllers and Views

> Do each controller together with its views before moving to the next one.

- [x] **HomeController** → `Controllers/HomeController.cs` + **Home/Index.cshtml** → `Views/Home/Index.cshtml`

  Controller `Index()`:
  - [x] Call `RankingService.GetTopRolesAsync(10)` and `GetTopSkillsAsync(10)`
  - [x] Call `JobService.GetRecentAsync(6)`
  - [x] If `User.Identity.IsAuthenticated` → call `RecommendationService.GetRecommendedJobsAsync(userId, 6)`
  - [x] Build `HomeViewModel` → return `View(vm)`

  View sections:
  - [x] Hero: headline, subheadline, search bar (posts to `/Jobs`), 4 stat counter cards with `data-count-to` attributes
  - [x] Trending Roles: horizontal bar chart — inject model data into JS via `@Html.Raw(Json.Serialize(Model.TopRoles))`
  - [x] Top Skills: pill cloud — loop `@foreach (var skill in Model.TopSkills)`, vary Tailwind text size class based on `TotalJobMentions`
  - [x] Recent Jobs: `@foreach (var job in Model.RecentJobs) { @await Html.PartialAsync("_JobCard", job) }` in a 3-column grid
  - [x] Locked section: `@if (!User.Identity?.IsAuthenticated ?? true)` — show blurred overlay + "Create Free Account" CTA

---

- [x] **JobsController** → `Controllers/JobsController.cs`

  Actions:
  - [x] `Index` (GET `/Jobs`): read query params → `JobService.SearchAsync(...)` → build `JobSearchViewModel` → if `Request.Headers.ContainsKey("HX-Request")` return `PartialView("_JobResults", vm)` else return `View(vm)`
  - [x] `Detail` (GET `/Jobs/Detail/{id}`): `GetByIdAsync(id)` → if null return `NotFound()` → check `IsJobSavedAsync` if authenticated → `JobDetailViewModel` → `View(vm)`
  - [x] `Save` (POST, `[Authorize]`): `JobService.SaveJobAsync(userId, jobId)` → `Ok()`
  - [x] `Unsave` (POST, `[Authorize]`): `JobService.UnsaveJobAsync(userId, jobId)` → `Ok()`

- [x] **Jobs/Index.cshtml** → `Views/Jobs/Index.cshtml` | [x] **Jobs/_JobResults.cshtml** → `Views/Jobs/_JobResults.cshtml`

  | Jobs/Index.cshtml | Jobs/_JobResults.cshtml |
  |---|---|
  | [x] 2-column layout: 280px left sidebar + scrollable right results | [x] Only renders the results list + pagination — NOT the sidebar |
  | [x] Left sidebar: search input with HTMX (`hx-get="/Jobs"` `hx-trigger="keyup changed delay:400ms"` `hx-target="#job-results"` `hx-include` for all filter fields), location dropdown, experience level toggle pills, salary min/max inputs, skills checkboxes, Apply Filters button | [x] When HTMX triggers a search, controller returns this partial and HTMX swaps it into `#job-results` — without this file, live search would replace the entire page including the sidebar |
  | [x] Right: results count header, sort dropdown, `<div id="job-results">@await Html.PartialAsync("_JobResults", Model)</div>`, pagination | [x] `@model JobSearchViewModel` — loop jobs with `_JobCard` partial, render pagination links |

- [x] **Jobs/Detail.cshtml** → `Views/Jobs/Detail.cshtml`
  - [x] Header card: job title (large), company/location/date row, badges row (experience level + source site + active dot), two action buttons ("Visit Original Posting ↗" and "♡ Save Job"), demand score bar (0–100 filled progress bar)
  - [x] 2-column body:
    - [x] Left (60%): "About This Role" placeholder paragraphs, required skills pills (each links to `/Rankings/RoleDetail?title={skill.Name}`), salary intelligence card (posted range vs market average with a visual marker on a range bar, data point count)
    - [x] Right (40%): "Similar Jobs" — 4 compact `_JobCard` partials, "Role Skill Demand" mini Chart.js bar chart (top 5 skills for this role)
  - [x] All Chart.js code goes in `@section Scripts { <script>...</script> }`

---

- [x] **RankingsController** → `Controllers/RankingsController.cs`

  Actions:
  - [x] `Index` (GET `/Rankings`): `GetTopRolesAsync(15)` + `GetTopSkillsAsync(15)` → `RankingsViewModel` → `View(vm)`
  - [x] `RoleDetail` (GET `/Rankings/RoleDetail?title=X`): `GetRoleTrendAsync(title)` + `GetSalaryRangeAsync(title)` + `SearchAsync(query: title, page:1, pageSize:6)` + skills query → `RoleDetailViewModel` → `View(vm)`

- [x] **Rankings/Index.cshtml** → `Views/Rankings/Index.cshtml` | [x] **Rankings/RoleDetail.cshtml** → `Views/Rankings/RoleDetail.cshtml`

  | Rankings/Index.cshtml | Rankings/RoleDetail.cshtml |
  |---|---|
  | [x] Section 1: large horizontal bar chart (Chart.js) of top 15 roles. Inject: `var labels = @Html.Raw(Json.Serialize(Model.TopRoles.Select(r => r.Title)))` and `var data = @Html.Raw(Json.Serialize(Model.TopRoles.Select(r => r.Count)))`. Week/month/quarter tab pills (visual toggle only). Below chart: data table (Rank / Role / Postings / Avg Salary / Trend arrow). Rows are links to RoleDetail. | [x] 3 stat cards at top: postings count, avg salary, demand score |
  | [x] Section 2: 5-column grid of skill cards — name, category badge, `TotalJobMentions` count (large monospace number), demand tier badge. Each card links to `/Rankings/RoleDetail?title={skill.Name}` | [x] Panel 1: demand trend line chart (labels = `SnapshotDate`, data = `PostingCount`) |
  | | [x] Panel 2: salary breakdown table by experience level |
  | | [x] Panel 3: top skills bar chart (Chart.js) for this specific role |
  | | [x] Panel 4: location table (city / postings / avg salary) |
  | | [x] Panel 5: 6 job cards in 2-column grid using `_JobCard` partial |
  | | [x] All 3 charts in `@section Scripts` |

---

## Phase 6 — Authentication

- [x] **AccountController** → `Controllers/AccountController.cs`
  - [x] Inject `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`
  - [x] `Register` (GET): return `View(new RegisterViewModel())`
  - [x] `Register` (POST): validate ModelState → create `ApplicationUser` (set Email, UserName, DisplayName, CreatedAt) → `CreateAsync(user, password)` → if succeeded: `SignInAsync` → redirect to Home → if failed: add errors to ModelState → return `View(vm)`
  - [x] `Login` (GET): return `View(new LoginViewModel())`
  - [x] `Login` (POST): validate ModelState → `PasswordSignInAsync` → if succeeded: redirect to Home → if failed: `ModelState.AddModelError("", "Invalid email or password")` → return `View(vm)`
  - [x] `Logout` (POST): `SignOutAsync()` → redirect to Home

- [x] **Account/Register.cshtml** → `Views/Account/Register.cshtml` | [x] **Account/Login.cshtml** → `Views/Account/Login.cshtml`

  | Register.cshtml | Login.cshtml |
  |---|---|
  | [x] `@model RegisterViewModel` | [x] `@model LoginViewModel` |
  | [x] Centered card (max-width 480px) | [x] Same centered card layout |
  | [x] Fields with `asp-for` + `asp-validation-for`: Display Name, Email, Password, Confirm Password | [x] Fields: Email, Password |
  | [x] Password strength indicator bar (CSS + small JS on input event) | [x] "Remember me" checkbox bound to `bool RememberMe` |
  | [x] `<div asp-validation-summary="ModelOnly">` for server errors | [x] "Forgot password?" link |
  | [x] "Create Account" full-width button | [x] `<div asp-validation-summary="ModelOnly">` for "Invalid email or password" error |
  | [x] Link: "Already have an account? Sign in" | [x] "Sign In" full-width button |
  | | [x] Link: "New here? Create a free account" |

> **Checkpoint:** Register a new user → verify row appears in AspNetUsers table → log in → verify session persists on page reload → log out → verify session is gone.

---

## Phase 7 — User Profile and Personalization

- [x] **ProfileController** → `Controllers/ProfileController.cs`
  - [x] Decorate the entire controller with `[Authorize]`
  - [x] `Index` (GET): load current user via `_userManager.GetUserAsync(User)` → load their `UserSkills` → load all `Skills` → load their `SavedJobs` with Job included → build `ProfileViewModel` → `View(vm)`
  - [x] `SaveProfile` (POST): update `DisplayName` and `ExperienceLevel` on the user → `UpdateAsync` → delete all existing `UserSkills` for this user → insert new `UserSkill` rows for each selected SkillId → `SaveChangesAsync` → `TempData["Success"] = "Profile saved"` → redirect to `Index`
  - [x] **Added Photo Feature:** Handle `ProfilePicture` upload in `SaveProfile`, save to `wwwroot/uploads/profiles`, and update `ProfilePicturePath`.
  - [x] `SavedJobs` (GET): load `SavedJobs` where UserId matches, Include Job navigation → `View(savedJobs)`

- [x] **Profile/Index.cshtml** → `Views/Profile/Index.cshtml` | [x] **Profile/SavedJobs.cshtml** → `Views/Profile/SavedJobs.cshtml`

  | Profile/Index.cshtml | Profile/SavedJobs.cshtml |
  |---|---|
  | [x] 2-column layout: 260px left sidebar + right content | [x] List of saved jobs |
  | [x] Left: initials avatar circle or **uploaded photo**, name, email, "Member since" date, 2 mini stat cards (saved jobs count / skills count), navigation links | [x] Loop `@foreach (var saved in Model)` → render `_JobCard` partial with an extra unsave form button overlaid |
  | [x] Right — Skill multi-select: loop all 25 skills as clickable pills. JS toggles a CSS class (selected = blue filled, unselected = outlined). | [x] Unsave button: small HTMX form to `/Jobs/Unsave` |
  | [x] Right — Experience level: dropdown bound to `ExperienceLevel` | [x] Empty state: `@if (!Model.Any())` → show "No saved jobs yet" message + link to `/Jobs` |
  | [x] Right — **Profile Picture Upload**: input field with file type validation | |
  | [x] "Save Profile" submit button | |

- [x] **SalaryController** → `Controllers/SalaryController.cs` + [x] **Salary/Submit.cshtml** → `Views/Salary/Submit.cshtml`
  - [x] Controller: `Submit` (GET) → `View(new SalarySubmitViewModel())`. `Submit` (POST) → validate ModelState → map to `SalaryReport` entity (set `UserId` from `_userManager.GetUserId(User)` if authenticated, else null for anonymous) → `SalaryService.SubmitReportAsync(report)` → `TempData["Success"] = "Thank you — your data helps the community"` → redirect to `Submit` GET
  - [x] View: Job Title input, Location input, Salary input + currency select (USD/EGP), Years of Experience slider (1–40, JS live value display next to slider), Skills comma-separated text input, "Submit Anonymously" note, submit button

> **Checkpoint:** Log in → set 5 skills → reload home page → personalized feed section shows jobs matching those skills. Submit a salary report → verify row in SalaryReports table with correct UserId.

---

## Phase 8 — Connect Real Data

- [x] **Update database with expanded realistic data**
  - [x] Expanded `DbSeeder.cs` to include 52 skills across 9 categories.
  - [x] Generated 500+ realistic jobs across Cairo, Dubai, Riyadh, and Remote.
  - [x] Created 12-week historical demand snapshots for top roles.
  - [x] Added 150+ community salary reports for data blending.
  - [x] Verified data integrity: Skills=52, Jobs=500, JobSkills=~2300.

---

## Phase 9 — Polish (only after Phases 1–7 are solid)

> These are independent of each other — do in any order.

---

| | |
|---|---|
| **HTMX live filters on sidebar** → `Views/Jobs/Index.cshtml` | **Animated stat counters on home page** → `Views/Home/Index.cshtml` |
| Add `hx-get`, `hx-trigger="change"`, `hx-include` to the location dropdown, experience pills, and salary inputs — not just the search text input | Add `data-count-to="24800"` attributes to each stat card number span |
| Result: changing any filter updates the results list without a full page reload | JS snippet in `@section Scripts`: on DOMContentLoaded, find all `[data-count-to]` elements, animate from 0 to target over 1.5s using `requestAnimationFrame` |
| Small change, large UX improvement | No library needed — ~15 lines of plain JS |

---

| | |
|---|---|
| **Empty states on all pages** → multiple view files | **Mobile responsive pass** → all view files |
| Job search: `@if (!Model.Jobs.Any())` → "No jobs found — try removing some filters" | Go through every page at 375px width in browser DevTools |
| Saved jobs: `@if (!Model.Any())` → "You haven't saved any jobs yet" + link to `/Jobs` | Sidebar on Jobs page: Flowbite drawer component for mobile |
| Home personalized feed: `@if (Model.PersonalizedJobs?.Any() != true)` → "Add skills to your profile to see matched jobs" | 3-column grids: add `grid-cols-1 md:grid-cols-2 lg:grid-cols-3` |
| Role detail charts: if `Model.Trend.Count == 0` → show "Not enough data yet" placeholder | Job detail 2-column: add `flex-col md:flex-row` |
| | Navbar: Flowbite navbar collapse with hamburger already built in |

---

| | |
|---|---|
| **HTMX loading skeleton** → `Views/Jobs/Index.cshtml` + `Views/Jobs/_JobResults.cshtml` | **TempData flash messages** → `Views/Shared/_Layout.cshtml` + all POST controller actions |
| Listen to `htmx:beforeRequest` on the results div → add a CSS class that shows skeleton loader cards | In `_Layout.cshtml`: `@if (TempData["Success"] != null)` → render Flowbite toast/alert with success message |
| Listen to `htmx:afterRequest` → remove the skeleton class | `@if (TempData["Error"] != null)` → render error toast |
| Alternatively: put Flowbite skeleton HTML inside `#job-results` as initial content — HTMX replaces it on first load | In controller POST actions after redirect: set `TempData["Success"] = "..."` or `TempData["Error"] = "..."` |
| Gives users visual feedback that filtering is working | Covers: profile saved, salary submitted, login failed, save job failed |

---

*Total: 52 tasks across 9 phases. Phases 1–7 are the full working product. Phase 8 is the real data switchover. Phase 9 is polish.*

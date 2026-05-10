# JobPulse — Solo Developer Task List

> Layout guide: tasks stacked vertically = do in order (each depends on the one above).
> Tasks side by side in the same block = can be done in any order or at the same time.

---

## Phase 1 — Project Foundation

- [ ] **Create the ASP.NET MVC Core project**
  - Run `dotnet new mvc -n JobPulse`
  - Verify it builds and runs a blank page before touching anything else
  - This is the root — every file lives inside it

- [ ] **Install all NuGet packages**
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - Nothing else works without these three

- [ ] **Write all entity classes** → `Models/Entities/*.cs`
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

- [ ] **Write ApplicationDbContext** → `Data/ApplicationDbContext.cs`
  - Extends `IdentityDbContext<ApplicationUser>`
  - Register all DbSets (Jobs, Skills, JobSkills, UserSkills, DemandSnapshots, SalaryReports, SavedJobs)
  - Configure in `OnModelCreating`:
    - Unique index on SavedJob(UserId + JobId)
    - Unique index on UserSkill(UserId + SkillId)
    - Decimal precision (10,2) on all salary columns
    - Map pipeline tables to their exact table names with `.ToTable()`
  - This is the bridge between C# code and the database — everything reads/writes through here

- [ ] **Configure Program.cs** → `Program.cs`
  - `AddDbContext` with SQL Server connection string
  - `AddIdentity<ApplicationUser, IdentityRole>` → `AddEntityFrameworkStores<ApplicationDbContext>`
  - `AddScoped` for each service interface (IJobService, IRankingService, IRecommendationService, ISalaryService)
  - Middleware order (MUST be exactly this): `UseStaticFiles` → `UseRouting` → `UseAuthentication` → `UseAuthorization`
  - Wrong middleware order causes silent auth bugs

- [ ] **Set connection string** → `appsettings.json`
  - Add `ConnectionStrings.DefaultConnection` pointing to local SQL Server
  - Use `TrustServerCertificate=True` for dev
  - Never hardcode the connection string anywhere in C# files

- [ ] **Run first EF migration and update database**
  - `dotnet ef migrations add InitialCreate`
  - `dotnet ef database update`
  - Open SSMS or Azure Data Studio and verify every table exists with correct columns
  - If anything is wrong: fix the entity → delete the migration folder → re-run both commands
  - Files auto-generated in `Data/Migrations/` — never edit manually

- [ ] **Write mock data seeder** → `Data/DbSeeder.cs`
  - Static class `DbSeeder` with `SeedAsync(ApplicationDbContext db)` method
  - Call it from `Program.cs` on startup only if the DB is empty
  - Seed:
    - 25 canonical Skills (Python, JavaScript, React, SQL, Docker, AWS, TypeScript, Node.js, Kubernetes, Git, .NET, Java, PostgreSQL, GraphQL, Flutter, C#, Angular, Vue, Redis, MongoDB, Azure, Kotlin, Swift, Go, PHP)
    - 100 Jobs across 5 cities (Cairo, Alexandria, Dubai, Riyadh, Remote) and 15 roles
    - JobSkills linking each job to 3–6 skills
    - 20 DemandSnapshot rows (4 roles × 5 weekly snapshots each)
  - Without this, every page is blank and nothing can be tested

> **Checkpoint:** Write a throwaway test action that returns `db.Jobs.Take(5).ToList()` as JSON. If you see 5 rows, Phase 1 is done. Delete the test action before moving on.

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

---

| | |
|---|---|
| **IRecommendationService + RecommendationService** → `Services/RecommendationService.cs + IRecommendationService.cs` | **ISalaryService + SalaryService** → `Services/SalaryService.cs + ISalaryService.cs` |
| `GetRecommendedJobsAsync(userId, count)` — get user's SkillIds from UserSkills table → query Jobs where any JobSkill.SkillId is in that set → group by JobId and count overlapping skills → order by overlap count DESC → return top N. Pure LINQ, no ML needed. Powers the personalized feed on the home page. | `SubmitReportAsync(SalaryReport report)` — validate and insert into SalaryReports table. Set `SubmittedAt = DateTime.UtcNow` before saving. |
| | `GetReportsForRoleAsync(jobTitle)` — return all SalaryReports where JobTitle matches (case-insensitive). Used by RankingService to blend community data into salary averages. |

---

## Phase 3 — ViewModels

> Write all ViewModels before writing any controller or view. They define the data contract between the two.

---

| | | |
|---|---|---|
| **HomeViewModel** → `Models/ViewModels/HomeViewModel.cs` | **JobSearchViewModel** → `Models/ViewModels/JobSearchViewModel.cs` | **JobDetailViewModel** → `Models/ViewModels/JobDetailViewModel.cs` |
| `List<(string Title, int Count)> TopRoles` | `List<Job> Jobs` | `Job Job` |
| `List<Skill> TopSkills` | `int TotalCount` | `bool IsSaved` — toggled by the save button |
| `List<Job> RecentJobs` | `int CurrentPage` | `List<Job> SimilarJobs` |
| `List<Job>? PersonalizedJobs` — null when user is logged out. Null = show locked teaser section. Non-null = show personalized feed. | `int PageSize = 20` | |
| | `int TotalPages` — computed: `(int)Math.Ceiling((double)TotalCount / PageSize)` | |
| | Mirror all filter fields: `Query`, `Location`, `ExperienceLevel`, `SalaryMin`, `SalaryMax` — needed to repopulate the sidebar form after a search so it doesn't reset | |

---

| | | |
|---|---|---|
| **RankingsViewModel** → `Models/ViewModels/RankingsViewModel.cs` | **RoleDetailViewModel** → `Models/ViewModels/RoleDetailViewModel.cs` | **ProfileViewModel + SalarySubmitViewModel** → `Models/ViewModels/ProfileViewModel.cs` + `SalarySubmitViewModel.cs` |
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

- [ ] **_Layout.cshtml** → `Views/Shared/_Layout.cshtml`
  - The shell every page inherits via `@{ Layout = "_Layout"; }`
  - In `<head>`: Tailwind CDN (`<script src="https://cdn.tailwindcss.com">`), Flowbite CSS CDN
  - In `<body>`: `@await Html.PartialAsync("_Navbar")`, `<main>@RenderBody()</main>`
  - Before `</body>`: HTMX CDN, Flowbite JS CDN, Chart.js CDN, `@await RenderSectionAsync("Scripts", required: false)`
  - The Scripts section is critical — individual views inject their Chart.js code here
  - Getting this file right means every page automatically has all libraries

- [ ] **_Navbar.cshtml** → `Views/Shared/_Navbar.cshtml` | **_JobCard.cshtml** → `Views/Shared/_JobCard.cshtml`

  | _Navbar.cshtml | _JobCard.cshtml |
  |---|---|
  | Rendered inside `_Layout.cshtml` via `@await Html.PartialAsync("_Navbar")` | `@model Job` — takes a Job entity, renders a single card |
  | Left: logo/wordmark | Left 3px accent bar (color encodes demand) |
  | Center: nav links (Home / Jobs / Rankings) | Job title, company, location, salary range (monospace) |
  | Right: conditional on `@User.Identity?.IsAuthenticated` — logged out = "Sign In" + "Get Started" buttons, logged in = avatar + dropdown (Profile, Saved Jobs, Sign Out) | Up to 4 skill pills + source badge + posted date + demand badge |
  | Active link highlighted using `ViewContext.RouteData.Values["controller"]` comparison | Save bookmark icon + "View Details →" button |
  | Mobile: Flowbite navbar collapse with hamburger | Used on home, search results, and role detail pages — write once, reuse everywhere via `@await Html.PartialAsync("_JobCard", job)` |

---

## Phase 5 — Controllers and Views

> Do each controller together with its views before moving to the next one.

- [ ] **HomeController** → `Controllers/HomeController.cs` + **Home/Index.cshtml** → `Views/Home/Index.cshtml`

  Controller `Index()`:
  - Call `RankingService.GetTopRolesAsync(10)` and `GetTopSkillsAsync(10)`
  - Call `JobService.GetRecentAsync(6)`
  - If `User.Identity.IsAuthenticated` → call `RecommendationService.GetRecommendedJobsAsync(userId, 6)`
  - Build `HomeViewModel` → return `View(vm)`

  View sections:
  - Hero: headline, subheadline, search bar (posts to `/Jobs`), 4 stat counter cards with `data-count-to` attributes
  - Trending Roles: horizontal bar chart — inject model data into JS via `@Html.Raw(Json.Serialize(Model.TopRoles))`
  - Top Skills: pill cloud — loop `@foreach (var skill in Model.TopSkills)`, vary Tailwind text size class based on `TotalJobMentions`
  - Recent Jobs: `@foreach (var job in Model.RecentJobs) { @await Html.PartialAsync("_JobCard", job) }` in a 3-column grid
  - Locked section: `@if (!User.Identity?.IsAuthenticated ?? true)` — show blurred overlay + "Create Free Account" CTA

---

- [ ] **JobsController** → `Controllers/JobsController.cs`

  Actions:
  - `Index` (GET `/Jobs`): read query params → `JobService.SearchAsync(...)` → build `JobSearchViewModel` → if `Request.Headers.ContainsKey("HX-Request")` return `PartialView("_JobResults", vm)` else return `View(vm)`
  - `Detail` (GET `/Jobs/Detail/{id}`): `GetByIdAsync(id)` → if null return `NotFound()` → check `IsJobSavedAsync` if authenticated → `JobDetailViewModel` → `View(vm)`
  - `Save` (POST, `[Authorize]`): `JobService.SaveJobAsync(userId, jobId)` → `Ok()`
  - `Unsave` (POST, `[Authorize]`): `JobService.UnsaveJobAsync(userId, jobId)` → `Ok()`

- [ ] **Jobs/Index.cshtml** → `Views/Jobs/Index.cshtml` | **Jobs/_JobResults.cshtml** → `Views/Jobs/_JobResults.cshtml`

  | Jobs/Index.cshtml | Jobs/_JobResults.cshtml |
  |---|---|
  | 2-column layout: 280px left sidebar + scrollable right results | Only renders the results list + pagination — NOT the sidebar |
  | Left sidebar: search input with HTMX (`hx-get="/Jobs"` `hx-trigger="keyup changed delay:400ms"` `hx-target="#job-results"` `hx-include` for all filter fields), location dropdown, experience level toggle pills, salary min/max inputs, skills checkboxes, Apply Filters button | When HTMX triggers a search, controller returns this partial and HTMX swaps it into `#job-results` — without this file, live search would replace the entire page including the sidebar |
  | Right: results count header, sort dropdown, `<div id="job-results">@await Html.PartialAsync("_JobResults", Model)</div>`, pagination | `@model JobSearchViewModel` — loop jobs with `_JobCard` partial, render pagination links |

- [ ] **Jobs/Detail.cshtml** → `Views/Jobs/Detail.cshtml`
  - Header card: job title (large), company/location/date row, badges row (experience level + source site + active dot), two action buttons ("Visit Original Posting ↗" and "♡ Save Job"), demand score bar (0–100 filled progress bar)
  - 2-column body:
    - Left (60%): "About This Role" placeholder paragraphs, required skills pills (each links to `/Rankings/RoleDetail?title={skill.Name}`), salary intelligence card (posted range vs market average with a visual marker on a range bar, data point count)
    - Right (40%): "Similar Jobs" — 4 compact `_JobCard` partials, "Role Skill Demand" mini Chart.js bar chart (top 5 skills for this role)
  - All Chart.js code goes in `@section Scripts { <script>...</script> }`

---

- [ ] **RankingsController** → `Controllers/RankingsController.cs`

  Actions:
  - `Index` (GET `/Rankings`): `GetTopRolesAsync(15)` + `GetTopSkillsAsync(15)` → `RankingsViewModel` → `View(vm)`
  - `RoleDetail` (GET `/Rankings/RoleDetail?title=X`): `GetRoleTrendAsync(title)` + `GetSalaryRangeAsync(title)` + `SearchAsync(query: title, page:1, pageSize:6)` + skills query → `RoleDetailViewModel` → `View(vm)`

- [ ] **Rankings/Index.cshtml** → `Views/Rankings/Index.cshtml` | **Rankings/RoleDetail.cshtml** → `Views/Rankings/RoleDetail.cshtml`

  | Rankings/Index.cshtml | Rankings/RoleDetail.cshtml |
  |---|---|
  | Section 1: large horizontal bar chart (Chart.js) of top 15 roles. Inject: `var labels = @Html.Raw(Json.Serialize(Model.TopRoles.Select(r => r.Title)))` and `var data = @Html.Raw(Json.Serialize(Model.TopRoles.Select(r => r.Count)))`. Week/month/quarter tab pills (visual toggle only). Below chart: data table (Rank / Role / Postings / Avg Salary / Trend arrow). Rows are links to RoleDetail. | 3 stat cards at top: postings count, avg salary, demand score |
  | Section 2: 5-column grid of skill cards — name, category badge, `TotalJobMentions` count (large monospace number), demand tier badge. Each card links to `/Rankings/RoleDetail?title={skill.Name}` | Panel 1: demand trend line chart (labels = `SnapshotDate`, data = `PostingCount`) |
  | | Panel 2: salary breakdown table by experience level |
  | | Panel 3: top skills bar chart (Chart.js) for this specific role |
  | | Panel 4: location table (city / postings / avg salary) |
  | | Panel 5: 6 job cards in 2-column grid using `_JobCard` partial |
  | | All 3 charts in `@section Scripts` |

---

## Phase 6 — Authentication

- [ ] **AccountController** → `Controllers/AccountController.cs`
  - Inject `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`
  - `Register` (GET): return `View(new RegisterViewModel())`
  - `Register` (POST): validate ModelState → create `ApplicationUser` (set Email, UserName, DisplayName, CreatedAt) → `CreateAsync(user, password)` → if succeeded: `SignInAsync` → redirect to `/Profile` → if failed: add errors to ModelState → return `View(vm)`
  - `Login` (GET): return `View(new LoginViewModel())`
  - `Login` (POST): validate ModelState → `FindByEmailAsync` → `PasswordSignInAsync` → if succeeded: redirect to Home → if failed: `ModelState.AddModelError("", "Invalid email or password")` → return `View(vm)`
  - `Logout` (POST): `SignOutAsync()` → redirect to Home

- [ ] **Account/Register.cshtml** → `Views/Account/Register.cshtml` | **Account/Login.cshtml** → `Views/Account/Login.cshtml`

  | Register.cshtml | Login.cshtml |
  |---|---|
  | `@model RegisterViewModel` | `@model LoginViewModel` |
  | Centered card (max-width 480px) | Same centered card layout |
  | Fields with `asp-for` + `asp-validation-for`: Display Name, Email, Password, Confirm Password | Fields: Email, Password |
  | Password strength indicator bar (CSS + small JS on input event) | "Remember me" checkbox bound to `bool RememberMe` |
  | `<div asp-validation-summary="ModelOnly">` for server errors | "Forgot password?" link (non-functional for now) |
  | "Create Account" full-width button | `<div asp-validation-summary="ModelOnly">` for "Invalid email or password" error |
  | Link: "Already have an account? Sign in" | "Sign In" full-width button |
  | | Link: "New here? Create a free account" |

> **Checkpoint:** Register a new user → verify row appears in AspNetUsers table → log in → verify session persists on page reload → log out → verify session is gone.

---

## Phase 7 — User Profile and Personalization

- [ ] **ProfileController** → `Controllers/ProfileController.cs`
  - Decorate the entire controller with `[Authorize]`
  - `Index` (GET): load current user via `_userManager.GetUserAsync(User)` → load their `UserSkills` → load all `Skills` → load their `SavedJobs` with Job included → build `ProfileViewModel` → `View(vm)`
  - `SaveProfile` (POST): update `DisplayName` and `ExperienceLevel` on the user → `UpdateAsync` → delete all existing `UserSkills` for this user → insert new `UserSkill` rows for each selected SkillId → `SaveChangesAsync` → `TempData["Success"] = "Profile saved"` → redirect to `Index`
  - `SavedJobs` (GET): load `SavedJobs` where UserId matches, Include Job navigation → `View(savedJobs)`

- [ ] **Profile/Index.cshtml** → `Views/Profile/Index.cshtml` | **Profile/SavedJobs.cshtml** → `Views/Profile/SavedJobs.cshtml`

  | Profile/Index.cshtml | Profile/SavedJobs.cshtml |
  |---|---|
  | 2-column layout: 260px left sidebar + right content | List of saved jobs |
  | Left: initials avatar circle (first letter of DisplayName, colored background), name, email, "Member since" date, 3 mini stat cards (saved jobs count / salary reports count / skills count), navigation links | Loop `@foreach (var saved in Model)` → render `_JobCard` partial with an extra unsave form button overlaid |
  | Right — Skill multi-select: loop all 25 skills as clickable pills. JS toggles a CSS class (selected = blue filled, unselected = outlined). On toggle, update a hidden `<input name="SelectedSkillIds" type="hidden">` list. On form submit, these hidden inputs are sent to `SaveProfile` POST. | Unsave button: small form `<form method="post" action="/Jobs/Unsave"><input name="jobId" value="@saved.JobId"/><button>Remove</button></form>` |
  | Right — Experience level: 4 radio buttons (Entry / Mid / Senior / Lead) bound to `ExperienceLevel` | Empty state: `@if (!Model.Any())` → show "No saved jobs yet" message + link to `/Jobs` |
  | "Save Profile" submit button | |

- [ ] **SalaryController** → `Controllers/SalaryController.cs` + **Salary/Submit.cshtml** → `Views/Salary/Submit.cshtml`
  - Controller: `Submit` (GET) → `View(new SalarySubmitViewModel())`. `Submit` (POST) → validate ModelState → map to `SalaryReport` entity (set `UserId` from `_userManager.GetUserId(User)` if authenticated, else null for anonymous) → `SalaryService.SubmitReportAsync(report)` → `TempData["Success"] = "Thank you — your data helps the community"` → redirect to `Submit` GET
  - View: Job Title input, Location input, Salary input + currency select (USD/EGP), Years of Experience slider (1–40, JS live value display next to slider), Skills comma-separated text input, "Submit Anonymously" note (small muted text explaining data is anonymous), submit button

> **Checkpoint:** Log in → set 5 skills → reload home page → personalized feed section shows jobs matching those skills. Submit a salary report → verify row in SalaryReports table with correct UserId.

---

## Phase 8 — Connect Real Data (do when friend delivers the pipeline)

- [ ] **Update connection string to shared DB** → `appsettings.json`
  - Point `DefaultConnection` to the shared SQL Server instance
  - Verify table names match what the pipeline created — if different, use `[Table("ActualName")]` attribute on entity classes

- [ ] **Verify skill normalization** | **Smoke test every page with real data**

  | Verify skill normalization | Smoke test every page |
  |---|---|
  | Run: `SELECT DISTINCT Name FROM Skills ORDER BY Name` | Visit every page, check for null reference exceptions |
  | If "python", "Python3", "Python developer" all appear as separate rows → normalization is broken → flag to data engineer before building anything else | Check rankings page — if no DemandSnapshot data, trend chart should show empty state not crash |
  | Confirm canonical names match the 25 skills you seeded (case-sensitive) | Check salary columns — if all show $0, the pipeline may use different column names |
  | | Add `@if (Model.X != null)` null guards in any view that crashes |

- [ ] **Disable the DbSeeder** → `Data/DbSeeder.cs`
  - Comment out or remove the seeder call in `Program.cs` once real data is confirmed working
  - Keep the `DbSeeder.cs` file — useful for local dev and testing

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

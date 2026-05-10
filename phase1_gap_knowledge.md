# Phase 1 Gap Knowledge

Phase 1 is mostly covered by the course, but a few parts require extra study beyond the slides.

## Covered By The Course

- ASP.NET Core MVC project structure: controllers, views, routing, `_Layout.cshtml`, and `RenderBody`.
- Entity Framework Core Code First basics: entity classes, `DbContext`, `DbSet`, migrations, database update, and connection strings in `appsettings.json`.
- Relationships and navigation properties: `ICollection<T>` and foreign-key-style model relationships.
- Basic EF writes: adding rows and saving changes.

## Knowledge Gaps Or Partial Gaps

### ASP.NET Core Identity

Used in Phase 1 by:
- `ApplicationUser : IdentityUser`
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>`
- `AddIdentity<ApplicationUser, IdentityRole>()`

Why it is a gap:
The knowledge gap analysis says Identity was not covered in the course. Registration, login, logout, `UserManager`, `SignInManager`, `[Authorize]`, and Identity tables are new material.

### Dependency Injection And Service Registration

Used in Phase 1 by:
- `builder.Services.AddScoped<IJobService, JobService>()`
- `builder.Services.AddScoped<IRankingService, RankingService>()`
- `builder.Services.AddScoped<IRecommendationService, RecommendationService>()`
- `builder.Services.AddScoped<ISalaryService, SalaryService>()`

Why it is a gap:
The course shows `AddDbContext`, but not interface-based service registration or constructor injection as a full app architecture.

### Fluent API In `OnModelCreating`

Used in Phase 1 by:
- `.HasIndex(...).IsUnique()`
- `.HasPrecision(10, 2)`
- `.ToTable(...)`

Why it is a partial gap:
The course covers data annotations like `[Key]` and `[ForeignKey]`, but the gap file says Fluent API configuration is only partially covered.

### Async Startup Seeding

Used in Phase 1 by:
- `DbSeeder.SeedAsync(ApplicationDbContext db)`
- `await db.SaveChangesAsync()`
- Startup seeding through a scoped `ApplicationDbContext`

Why it is a partial gap:
The course only lightly covers async patterns. The app uses async EF calls and startup integration more heavily.

### SQLite Provider Change

Used in Phase 1 by:
- `Microsoft.EntityFrameworkCore.Sqlite`
- `options.UseSqlite(...)`
- `Data Source=JobPulse.db`

Why it matters:
The original project plan is SQL Server, but SQLite is being used temporarily for DB Browser. EF concepts are the same, but provider-specific behavior can differ.

## Phase 1 Checkpoint Result

The temporary test action returned 5 rows from:

```csharp
db.Jobs.Take(5).ToList()
```

The action was removed after verification.

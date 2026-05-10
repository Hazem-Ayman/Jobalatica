using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ISalaryService, SalaryService>();

var app = builder.Build();
var seedOnly = args.Contains("--seed-only");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(db);

    if (seedOnly)
    {
        var skillCount = await db.Skills.CountAsync();
        var jobCount = await db.Jobs.CountAsync();
        var jobSkillCount = await db.JobSkills.CountAsync();
        var snapshotCount = await db.DemandSnapshots.CountAsync();

        Console.WriteLine($"Seed check: Skills={skillCount}, Jobs={jobCount}, JobSkills={jobSkillCount}, DemandSnapshots={snapshotCount}");
    }
}

if (seedOnly)
{
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

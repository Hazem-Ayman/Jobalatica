using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    builder.Services.AddControllersWithViews();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=JobPulse.db";
    if (connectionString.Contains("Data Source=") && !connectionString.Contains(":memory:") && !Path.IsPathRooted(connectionString.Split('=')[1]))
    {
        var dbFile = connectionString.Split('=')[1];
        connectionString = $"Data Source={Path.Combine(builder.Environment.ContentRootPath, dbFile)}";
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Brevo email sender
    builder.Services.Configure<BrevoSettings>(builder.Configuration.GetSection("Brevo"));
    builder.Services.AddTransient<IEmailSender, EmailSender>();

    builder.Services.AddScoped<IJobService, JobService>();
    builder.Services.AddScoped<IRankingService, RankingService>();
    builder.Services.AddScoped<IRecommendationService, RecommendationService>();
    builder.Services.AddScoped<ISalaryService, SalaryService>();

    var app = builder.Build();
    var seedOnly = args.Contains("--seed-only");
    var forceSeed = args.Contains("--force-seed");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbSeeder.SeedAsync(db, forceSeed);

        if (seedOnly || forceSeed)
        {
            var skillCount = await db.Skills.CountAsync();
            var jobCount = await db.Jobs.CountAsync();
            var jobSkillCount = await db.JobSkills.CountAsync();
            var snapshotCount = await db.DemandSnapshots.CountAsync();
            var reportCount = await db.SalaryReports.CountAsync();

            Console.WriteLine($"[SEED REPORT] Skills: {skillCount} | Jobs: {jobCount} | JobSkills: {jobSkillCount} | Reports: {reportCount} | Snapshots: {snapshotCount}");
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
    
    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    provider.Mappings[".avif"] = "image/avif";
    provider.Mappings[".webp"] = "image/webp";

    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider
    });

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    Console.WriteLine($"=== FATAL CRASH [{timestamp}] ===");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("===================");
    System.IO.File.AppendAllText("crash.log", $"\n[{timestamp}] FATAL STARTUP ERROR:\n{ex.ToString()}\n");
}

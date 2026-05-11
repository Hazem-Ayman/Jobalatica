using Jobalatica.Data;
using Jobalatica.Models.Entities;
using Jobalatica.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

try
{
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
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Brevo email sender
    builder.Services.Configure<BrevoSettings>(builder.Configuration.GetSection("Brevo"));
    builder.Services.AddTransient<IEmailSender, EmailSender>();

    // Allow file uploads up to 5MB
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 5242880; // 5MB
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 5242880; // 5MB
    });

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

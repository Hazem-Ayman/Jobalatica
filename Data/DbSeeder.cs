using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Jobs.CountAsync() > 200) return;

            var existingSkills = await db.Skills.ToListAsync();
            var newSkills = new List<Skill>();
            var skillNames = new Dictionary<string, string[]>
            {
                ["Language"] = new[] { "Python", "JavaScript", "TypeScript", "C#", "Java", "Go", "Swift", "Kotlin", "Rust", "PHP" },
                ["Framework"] = new[] { "React", "Angular", "Vue", ".NET Core", "Next.js", "Django", "FastAPI", "Spring Boot" },
                ["Database"] = new[] { "SQL Server", "PostgreSQL", "MongoDB", "Redis", "Elasticsearch", "MySQL" },
                ["DevOps"] = new[] { "Docker", "Kubernetes", "Terraform", "Jenkins", "GitHub Actions" },
                ["Cloud"] = new[] { "AWS", "Azure", "Google Cloud" },
                ["Mobile"] = new[] { "Flutter", "React Native", "iOS (Swift)", "Android (Kotlin)" },
                ["Testing"] = new[] { "Selenium", "Cypress", "Jest", "xUnit" },
                ["UI/UX"] = new[] { "Figma", "Adobe XD", "Tailwind CSS", "Bootstrap" },
                ["Management"] = new[] { "Jira", "Agile", "Scrum" }
            };

            foreach (var category in skillNames)
            {
                foreach (var name in category.Value)
                {
                    if (!existingSkills.Any(s => s.Name == name))
                        newSkills.Add(new Skill { Name = name, Category = category.Key });
                }
            }

            if (newSkills.Any())
            {
                db.Skills.AddRange(newSkills);
                await db.SaveChangesAsync();
                existingSkills = await db.Skills.ToListAsync();
            }

            var cities = new[] { "Cairo", "Alexandria", "Dubai", "Abu Dhabi", "Riyadh", "Jeddah", "Amman", "Remote" };
            var countries = new Dictionary<string, string>
            {
                ["Cairo"] = "Egypt", ["Alexandria"] = "Egypt", ["Dubai"] = "UAE", ["Abu Dhabi"] = "UAE",
                ["Riyadh"] = "Saudi Arabia", ["Jeddah"] = "Saudi Arabia", ["Amman"] = "Jordan", ["Remote"] = "Global"
            };

            var roles = new[]
            {
                "Senior Backend Engineer", "Frontend Architect", "Full Stack Developer", "Lead DevOps Engineer",
                "Data Scientist", "Mobile Lead (Flutter)", "Product Manager", "UI/UX Designer", "QA Automation Lead",
                "Security Specialist", "Cloud Solutions Architect", "Machine Learning Engineer"
            };

            var companies = new[]
            {
                "Vodafone", "Valeo", "Google", "Microsoft", "Amazon", "Careem", "Talabat", "Instabug", "Swvl", 
                "Fawry", "Paymob", "Bosta", "Thndr", "Breadfast", "Dubizzle", "Property Finder"
            };

            var experienceLevels = new[] { "Entry", "Mid", "Senior", "Lead" };
            var random = new Random();
            var jobs = new List<Job>();

            for (var i = 0; i < 400; i++)
            {
                var role = roles[random.Next(roles.Length)];
                var city = cities[random.Next(cities.Length)];
                decimal baseSalary = city == "Cairo" || city == "Alexandria" ? 15000 : 4000;
                decimal min = baseSalary + random.Next(2000, 15000);

                jobs.Add(new Job
                {
                    Title = role,
                    Company = companies[random.Next(companies.Length)],
                    Location = city,
                    Country = countries[city],
                    SalaryMin = min,
                    SalaryMax = min + random.Next(5000, 15000),
                    Currency = city == "Cairo" || city == "Alexandria" ? "EGP" : "USD",
                    ExperienceLevel = experienceLevels[random.Next(experienceLevels.Length)],
                    SourceUrl = $"https://example.com/jobs/{Guid.NewGuid()}",
                    SourceSite = i % 2 == 0 ? "LinkedIn" : "Wuzzuf",
                    PostedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
                    ScrapedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();

            var jobSkills = new List<JobSkill>();
            foreach (var job in jobs)
            {
                var selected = existingSkills.OrderBy(_ => random.Next()).Take(random.Next(3, 7)).ToList();
                foreach (var s in selected)
                {
                    jobSkills.Add(new JobSkill { JobId = job.Id, SkillId = s.Id });
                    s.TotalJobMentions++;
                }
            }
            db.JobSkills.AddRange(jobSkills);
            await db.SaveChangesAsync();

            var snapshots = new List<DemandSnapshot>();
            foreach (var role in roles.Take(8))
            {
                for (int w = 12; w >= 0; w--)
                {
                    snapshots.Add(new DemandSnapshot
                    {
                        JobTitle = role,
                        Location = "Global",
                        PostingCount = 50 + (w * random.Next(2, 10)) + random.Next(-5, 5),
                        AvgSalaryMin = 3000 + random.Next(500, 2000),
                        AvgSalaryMax = 6000 + random.Next(2000, 5000),
                        SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7 * w))
                    });
                }
            }
            db.DemandSnapshots.AddRange(snapshots);

            var reports = new List<SalaryReport>();
            for (int i = 0; i < 150; i++)
            {
                reports.Add(new SalaryReport
                {
                    JobTitle = roles[random.Next(roles.Length)],
                    Location = cities[random.Next(cities.Length)],
                    Salary = 5000 + random.Next(1000, 10000),
                    Currency = "USD",
                    YearsExperience = random.Next(1, 15),
                    SubmittedAt = DateTime.UtcNow.AddDays(-random.Next(0, 60)),
                    UserId = null
                });
            }
            db.SalaryReports.AddRange(reports);

            await db.SaveChangesAsync();
        }
    }
}

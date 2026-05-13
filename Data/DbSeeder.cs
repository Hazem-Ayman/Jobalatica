using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Jobs.CountAsync() > 1200) return;

            var existingSkills = await db.Skills.ToListAsync();
            var newSkills = new List<Skill>();
            var skillNames = new Dictionary<string, string[]>
            {
                ["Language"] = new[] { "Python", "JavaScript", "TypeScript", "C#", "Java", "Go", "Swift", "Kotlin", "Rust", "PHP", "Solidity", "Ruby", "C++" },
                ["Framework"] = new[] { "React", "Angular", "Vue", ".NET Core", "Next.js", "Django", "FastAPI", "Spring Boot", "Svelte", "Laravel", "Ruby on Rails", "Express" },
                ["Database"] = new[] { "SQL Server", "PostgreSQL", "MongoDB", "Redis", "Elasticsearch", "MySQL", "Cassandra", "DynamoDB" },
                ["DevOps"] = new[] { "Docker", "Kubernetes", "Terraform", "Jenkins", "GitHub Actions", "Ansible", "CircleCI", "Prometheus" },
                ["Cloud"] = new[] { "AWS", "Azure", "Google Cloud", "Cloudflare" },
                ["Mobile"] = new[] { "Flutter", "React Native", "iOS (Swift)", "Android (Kotlin)" },
                ["Testing"] = new[] { "Selenium", "Cypress", "Jest", "xUnit", "Playwright" },
                ["UI/UX"] = new[] { "Figma", "Adobe XD", "Tailwind CSS", "Bootstrap", "Framer" },
                ["Management"] = new[] { "Jira", "Agile", "Scrum", "Product Discovery" },
                ["Specialized"] = new[] { "PyTorch", "TensorFlow", "OpenCV", "Smart Contracts", "Penetration Testing" }
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

            var cities = new[] { "Cairo", "Alexandria", "Dubai", "Abu Dhabi", "Riyadh", "Jeddah", "Amman", "Kuwait City", "Doha", "Remote" };
            var countries = new Dictionary<string, string>
            {
                ["Cairo"] = "Egypt", ["Alexandria"] = "Egypt", ["Dubai"] = "UAE", ["Abu Dhabi"] = "UAE",
                ["Riyadh"] = "Saudi Arabia", ["Jeddah"] = "Saudi Arabia", ["Amman"] = "Jordan", 
                ["Kuwait City"] = "Kuwait", ["Doha"] = "Qatar", ["Remote"] = "Global"
            };

            var roles = new[]
            {
                "Senior Backend Engineer", "Frontend Architect", "Full Stack Developer", "Lead DevOps Engineer",
                "Data Scientist", "Mobile Lead (Flutter)", "Product Manager", "UI/UX Designer", "QA Automation Lead",
                "Security Specialist", "Cloud Solutions Architect", "Machine Learning Engineer", "Cybersecurity Analyst",
                "Blockchain Developer", "Data Engineer", "Site Reliability Engineer", "AI Research Scientist",
                "Embedded Systems Engineer", "Engineering Manager", "Solutions Architect"
            };

            var companies = new[]
            {
                "Noon", "Salla", "Foodics", "Hungerstation", "Tamara", "Tabby", "Careem", "Talabat", "Instabug", 
                "Swvl", "Fawry", "Paymob", "Bosta", "Thndr", "Breadfast", "Dubizzle", "Property Finder",
                "Google", "Microsoft", "Amazon", "Meta", "Netflix", "Stripe", "Airbnb", "Spotify"
            };

            var experienceLevels = new[] { "Entry", "Mid", "Senior", "Lead" };
            var random = new Random();
            var jobs = new List<Job>();

            for (var i = 0; i < 1000; i++)
            {
                var role = roles[random.Next(roles.Length)];
                var city = cities[random.Next(cities.Length)];
                
                // Realistic MENA/Gulf/Global Salary Scaling
                decimal baseMin, baseMax;
                if (city == "Cairo" || city == "Alexandria") {
                    baseMin = 18000 + random.Next(5000, 45000); // EGP
                    baseMax = baseMin + random.Next(10000, 30000);
                } else if (city == "Dubai" || city == "Abu Dhabi" || city == "Riyadh" || city == "Doha") {
                    baseMin = 3500 + random.Next(1500, 12000); // USD Equivalent
                    baseMax = baseMin + random.Next(2000, 8000);
                } else {
                    baseMin = 3000 + random.Next(2000, 15000); // Remote/Global USD
                    baseMax = baseMin + random.Next(3000, 10000);
                }

                jobs.Add(new Job
                {
                    Title = role,
                    Company = companies[random.Next(companies.Length)],
                    Location = city,
                    Country = countries[city],
                    SalaryMin = baseMin,
                    SalaryMax = baseMax,
                    Currency = (city == "Cairo" || city == "Alexandria") ? "EGP" : "USD",
                    ExperienceLevel = experienceLevels[random.Next(experienceLevels.Length)],
                    SourceUrl = $"https://example.com/signals/{Guid.NewGuid()}",
                    SourceSite = i % 3 == 0 ? "LinkedIn" : (i % 3 == 1 ? "Wuzzuf" : "GulfTalent"),
                    PostedAt = DateTime.UtcNow.AddDays(-random.Next(0, 45)),
                    ScrapedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();

            var jobSkills = new List<JobSkill>();
            foreach (var job in jobs)
            {
                var selected = existingSkills.OrderBy(_ => random.Next()).Take(random.Next(4, 9)).ToList();
                foreach (var s in selected)
                {
                    jobSkills.Add(new JobSkill { JobId = job.Id, SkillId = s.Id });
                    s.TotalJobMentions++;
                }
            }
            db.JobSkills.AddRange(jobSkills);
            await db.SaveChangesAsync();

            // Historical Trend Data
            var snapshots = new List<DemandSnapshot>();
            foreach (var role in roles)
            {
                for (int w = 24; w >= 0; w--) // 6 months of data
                {
                    snapshots.Add(new DemandSnapshot
                    {
                        JobTitle = role,
                        Location = "Global",
                        PostingCount = 120 + (w * random.Next(5, 15)) + random.Next(-10, 10),
                        AvgSalaryMin = 4000 + random.Next(500, 3000),
                        AvgSalaryMax = 7000 + random.Next(2000, 6000),
                        SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7 * w))
                    });
                }
            }
            db.DemandSnapshots.AddRange(snapshots);

            var reports = new List<SalaryReport>();
            for (int i = 0; i < 500; i++)
            {
                reports.Add(new SalaryReport
                {
                    JobTitle = roles[random.Next(roles.Length)],
                    Location = cities[random.Next(cities.Length)],
                    Salary = 4000 + random.Next(1000, 18000),
                    Currency = "USD",
                    YearsExperience = random.Next(1, 18),
                    SubmittedAt = DateTime.UtcNow.AddDays(-random.Next(0, 90)),
                    UserId = null
                });
            }
            db.SalaryReports.AddRange(reports);

            await db.SaveChangesAsync();
        }
    }
}

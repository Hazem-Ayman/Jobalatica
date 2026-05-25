using Jobalatica.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, bool force = false)
        {
            // If the user wants a fresh start, we clear the tables
            // We'll check if we need to seed by seeing if there are any jobs at all
            // or if we want to force it. For this update, we will clear if count is low or if we just want to refresh.
            
            bool shouldRefresh = force || await db.Jobs.CountAsync() <= 1200; // If only the old 1000 exist or forced

            if (shouldRefresh)
            {
                // Clear existing data to avoid duplicates and starting fresh
                await db.JobSkills.ExecuteDeleteAsync();
                await db.Jobs.ExecuteDeleteAsync();
                await db.DemandSnapshots.ExecuteDeleteAsync();
                await db.SalaryReports.ExecuteDeleteAsync();
                await db.SaveChangesAsync();
            }
            else if (await db.Jobs.CountAsync() > 4000) 
            {
                return; // Already seeded with the large set
            }

            var existingSkills = await db.Skills.ToListAsync();
            var newSkills = new List<Skill>();
            var skillNames = new Dictionary<string, string[]>
            {
                ["Language"] = new[] { "Python", "JavaScript", "TypeScript", "C#", "Java", "Go", "Swift", "Kotlin", "Rust", "PHP", "Solidity", "Ruby", "C++", "C", "Zig", "Scala", "Dart" },
                ["Framework"] = new[] { "React", "Angular", "Vue", ".NET Core", "Next.js", "Django", "FastAPI", "Spring Boot", "Svelte", "Laravel", "Ruby on Rails", "Express", "Remix", "Nuxt", "Flask", "Quarkus" },
                ["Database"] = new[] { "SQL Server", "PostgreSQL", "MongoDB", "Redis", "Elasticsearch", "MySQL", "Cassandra", "DynamoDB", "ClickHouse", "SurrealDB", "Supabase", "PlanetScale" },
                ["DevOps"] = new[] { "Docker", "Kubernetes", "Terraform", "Jenkins", "GitHub Actions", "Ansible", "CircleCI", "Prometheus", "Grafana", "ArgoCD", "Pulumi" },
                ["Cloud"] = new[] { "AWS", "Azure", "Google Cloud", "Cloudflare", "DigitalOcean", "Linode", "Vercel" },
                ["Mobile"] = new[] { "Flutter", "React Native", "iOS (Swift)", "Android (Kotlin)", "SwiftUI", "Jetpack Compose" },
                ["Testing"] = new[] { "Selenium", "Cypress", "Jest", "xUnit", "Playwright", "Vitest", "Testing Library" },
                ["UI/UX"] = new[] { "Figma", "Adobe XD", "Tailwind CSS", "Bootstrap", "Framer", "Chakra UI", "Shadcn UI", "Material UI" },
                ["Management"] = new[] { "Jira", "Agile", "Scrum", "Product Discovery", "Lean", "Kanban", "Linear" },
                ["Specialized"] = new[] { "PyTorch", "TensorFlow", "OpenCV", "Smart Contracts", "Penetration Testing", "LLM Fine-tuning", "LangChain", "RAG", "Data Engineering" }
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

            var cities = new[] { "Cairo", "Alexandria", "Dubai", "Abu Dhabi", "Riyadh", "Jeddah", "Amman", "Kuwait City", "Doha", "Remote", "New York", "London", "Berlin", "Singapore", "Tokyo", "San Francisco", "Austin", "Toronto", "Paris", "Amsterdam" };
            var countries = new Dictionary<string, string>
            {
                ["Cairo"] = "Egypt", ["Alexandria"] = "Egypt", ["Dubai"] = "UAE", ["Abu Dhabi"] = "UAE",
                ["Riyadh"] = "Saudi Arabia", ["Jeddah"] = "Saudi Arabia", ["Amman"] = "Jordan", 
                ["Kuwait City"] = "Kuwait", ["Doha"] = "Qatar", ["Remote"] = "Global",
                ["New York"] = "USA", ["London"] = "UK", ["Berlin"] = "Germany", ["Singapore"] = "Singapore",
                ["Tokyo"] = "Japan", ["San Francisco"] = "USA", ["Austin"] = "USA", ["Toronto"] = "Canada",
                ["Paris"] = "France", ["Amsterdam"] = "Netherlands"
            };

            var roles = new[]
            {
                "Senior Backend Engineer", "Frontend Architect", "Full Stack Developer", "Lead DevOps Engineer",
                "Data Scientist", "Mobile Lead (Flutter)", "Product Manager", "UI/UX Designer", "QA Automation Lead",
                "Security Specialist", "Cloud Solutions Architect", "Machine Learning Engineer", "Cybersecurity Analyst",
                "Blockchain Developer", "Data Engineer", "Site Reliability Engineer", "AI Research Scientist",
                "Embedded Systems Engineer", "Engineering Manager", "Solutions Architect", "Growth Lead",
                "iOS Developer", "Android Engineer", "Scrum Master", "Product Designer", "Cloud Engineer",
                "Security Engineer", "Data Analyst", "Database Administrator", "Technical Writer"
            };

            var companies = new[]
            {
                "Noon", "Salla", "Foodics", "Hungerstation", "Tamara", "Tabby", "Careem", "Talabat", "Instabug", 
                "Swvl", "Fawry", "Paymob", "Bosta", "Thndr", "Breadfast", "Dubizzle", "Property Finder",
                "Google", "Microsoft", "Amazon", "Meta", "Netflix", "Stripe", "Airbnb", "Spotify",
                "Oracle", "Salesforce", "Twitter", "Uber", "Lyft", "Pinterest", "Snapchat", "TikTok",
                "Shopify", "HubSpot", "Atlassian", "Twilio", "Adobe", "Intel", "NVIDIA"
            };

            var experienceLevels = new[] { "Entry", "Mid", "Senior", "Lead", "Director" };
            var random = new Random();
            var jobs = new List<Job>();

            // Generate 5000 random jobs
            for (var i = 0; i < 5000; i++)
            {
                var role = roles[random.Next(roles.Length)];
                var city = cities[random.Next(cities.Length)];
                var exp = experienceLevels[random.Next(experienceLevels.Length)];
                
                // Realistic Scaling based on location AND experience
                decimal expMultiplier = exp switch {
                    "Entry" => 1.0m,
                    "Mid" => 1.4m,
                    "Senior" => 1.9m,
                    "Lead" => 2.4m,
                    "Director" => 3.5m,
                    _ => 1.0m
                };

                decimal baseMin, baseMax;
                string currency;

                if (city == "Cairo" || city == "Alexandria") {
                    baseMin = (15000 + random.Next(0, 15000)) * expMultiplier; 
                    baseMax = baseMin + (random.Next(5000, 15000) * expMultiplier);
                    currency = "EGP";
                } else if (city == "Dubai" || city == "Abu Dhabi" || city == "Riyadh" || city == "Doha" || city == "Kuwait City") {
                    baseMin = (3000 + random.Next(0, 4000)) * expMultiplier; 
                    baseMax = baseMin + (random.Next(1000, 3000) * expMultiplier);
                    currency = "USD";
                } else if (new[] { "New York", "San Francisco", "Austin", "London", "Toronto", "Tokyo" }.Contains(city)) {
                    baseMin = (7000 + random.Next(0, 5000)) * expMultiplier; 
                    baseMax = baseMin + (random.Next(2000, 6000) * expMultiplier);
                    currency = "USD";
                } else {
                    baseMin = (2500 + random.Next(0, 3000)) * expMultiplier; 
                    baseMax = baseMin + (random.Next(1000, 4000) * expMultiplier);
                    currency = "USD";
                }

                var job = new Job
                {
                    Title = $"{exp} {role}",
                    Company = companies[random.Next(companies.Length)],
                    Location = city,
                    Country = countries[city],
                    SalaryMin = Math.Round(baseMin / 10) * 10,
                    SalaryMax = Math.Round(baseMax / 10) * 10,
                    Currency = currency,
                    ExperienceLevel = exp,
                    SourceUrl = $"https://example.com/signals/{Guid.NewGuid()}",
                    SourceSite = i % 4 == 0 ? "LinkedIn" : (i % 4 == 1 ? "Wuzzuf" : (i % 4 == 2 ? "GulfTalent" : "Indeed")),
                    PostedAt = DateTime.UtcNow.AddHours(-random.Next(1, 1000)), // Mostly within last 40 days
                    ScrapedAt = DateTime.UtcNow,
                    IsActive = true
                };

                jobs.Add(job);
            }

            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();

            // Distribute skills to jobs REALISTICALLY
            var jobSkills = new List<JobSkill>();
            foreach (var job in jobs)
            {
                var roleLower = job.Title.ToLower();
                var relevantCategories = new List<string>();
                
                if (roleLower.Contains("backend") || roleLower.Contains("engineer")) relevantCategories.AddRange(new[] { "Language", "Database", "DevOps", "Cloud" });
                if (roleLower.Contains("frontend") || roleLower.Contains("designer")) relevantCategories.AddRange(new[] { "Language", "UI/UX", "Framework" });
                if (roleLower.Contains("data") || roleLower.Contains("scientist") || roleLower.Contains("machine")) relevantCategories.AddRange(new[] { "Language", "Database", "Specialized" });
                if (roleLower.Contains("mobile") || roleLower.Contains("ios") || roleLower.Contains("android")) relevantCategories.AddRange(new[] { "Mobile", "Language", "UI/UX" });
                if (roleLower.Contains("security") || roleLower.Contains("cyber")) relevantCategories.AddRange(new[] { "Specialized", "DevOps", "Language" });
                if (roleLower.Contains("manager") || roleLower.Contains("product") || roleLower.Contains("scrum") || roleLower.Contains("lead")) relevantCategories.AddRange(new[] { "Management", "UI/UX" });
                
                if (!relevantCategories.Any()) relevantCategories.Add("Language");

                var pool = existingSkills.Where(s => relevantCategories.Contains(s.Category)).ToList();
                if (pool.Count < 3) pool = existingSkills;

                var selected = pool.OrderBy(_ => random.Next()).Take(random.Next(3, 7)).ToList();
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
                var trendFactor = random.NextDouble() * 2; // Some roles trending up, some down
                for (int w = 52; w >= 0; w--) 
                {
                    snapshots.Add(new DemandSnapshot
                    {
                        JobTitle = role,
                        Location = "Global",
                        PostingCount = (int)(150 + (w * trendFactor) + random.Next(-15, 15)),
                        AvgSalaryMin = 4500 + random.Next(0, 2000),
                        AvgSalaryMax = 8000 + random.Next(0, 4000),
                        SnapshotDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7 * w))
                    });
                }
            }
            db.DemandSnapshots.AddRange(snapshots);

            // Generate 2000 salary reports
            var reports = new List<SalaryReport>();
            for (int i = 0; i < 2000; i++)
            {
                var role = roles[random.Next(roles.Length)];
                var city = cities[random.Next(cities.Length)];
                var yearsExp = random.Next(1, 20);
                
                decimal multiplier = 1.0m + (yearsExp * 0.12m);
                decimal baseSal = (city == "Cairo" || city == "Alexandria") ? 18000 : 3500;

                reports.Add(new SalaryReport
                {
                    JobTitle = role,
                    Location = city,
                    Salary = Math.Round((baseSal + random.Next(0, 5000)) * multiplier / 100) * 100,
                    Currency = (city == "Cairo" || city == "Alexandria") ? "EGP" : "USD",
                    YearsExperience = yearsExp,
                    SubmittedAt = DateTime.UtcNow.AddDays(-random.Next(0, 180)),
                    UserId = null
                });
            }
            db.SalaryReports.AddRange(reports);

            await db.SaveChangesAsync();
        }
    }
}

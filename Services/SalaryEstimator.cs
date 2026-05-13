using Jobalatica.Models.Entities;

namespace Jobalatica.Services
{
    public interface ISalaryEstimator
    {
        /// <summary>Returns an estimate only when real salary is missing. Never modifies the job.</summary>
        SalaryEstimate? GetEstimate(Job job);
    }

    public class SalaryEstimate
    {
        public decimal EstimatedSalaryMin { get; set; }
        public decimal EstimatedSalaryMax { get; set; }
    }

    public class SalaryEstimator : ISalaryEstimator
    {
        private static readonly string[] SeniorityOrder = ["Intern", "Entry", "Mid", "Senior", "Lead"];

        private static readonly Dictionary<string, string[]> SeniorityKeywords = new()
        {
            ["Intern"] = ["intern", "trainee", "co-op", "coop"],
            ["Entry"] = ["junior", "jr", "jr.", "entry", "graduate", "fresher", "associate"],
            ["Mid"] = ["mid", "intermediate", "regular", "ii"],
            ["Senior"] = ["senior", "sr", "sr.", "experienced", "iii", "staff"],
            ["Lead"] = ["lead", "principal", "manager", "director", "head", "chief", "architect", "vp", "iv"],
        };

        private static readonly Dictionary<string, Dictionary<string, (decimal lo, decimal hi)>> SalaryBands = new()
        {
            ["Administrative"] = new() { ["Intern"] = (2000, 4000), ["Entry"] = (3000, 6000), ["Mid"] = (6000, 10000), ["Senior"] = (10000, 15000), ["Lead"] = (15000, 22000) },
            ["Accounting"] = new() { ["Intern"] = (3000, 5000), ["Entry"] = (5000, 10000), ["Mid"] = (10000, 18000), ["Senior"] = (18000, 30000), ["Lead"] = (28000, 50000) },
            ["Sales"] = new() { ["Intern"] = (2500, 4500), ["Entry"] = (4000, 8000), ["Mid"] = (8000, 15000), ["Senior"] = (15000, 28000), ["Lead"] = (25000, 45000) },
            ["Marketing"] = new() { ["Intern"] = (2500, 4500), ["Entry"] = (4000, 8000), ["Mid"] = (8000, 15000), ["Senior"] = (15000, 25000), ["Lead"] = (25000, 40000) },
            ["HR"] = new() { ["Intern"] = (2500, 4000), ["Entry"] = (4000, 7000), ["Mid"] = (7000, 14000), ["Senior"] = (14000, 22000), ["Lead"] = (22000, 35000) },
            ["Tech"] = new() { ["Intern"] = (3000, 6000), ["Entry"] = (6000, 12000), ["Mid"] = (12000, 22000), ["Senior"] = (22000, 40000), ["Lead"] = (40000, 70000) },
            ["Engineering"] = new() { ["Intern"] = (3000, 5000), ["Entry"] = (5000, 10000), ["Mid"] = (10000, 18000), ["Senior"] = (18000, 30000), ["Lead"] = (30000, 50000) },
            ["Healthcare"] = new() { ["Intern"] = (2500, 4500), ["Entry"] = (4000, 8000), ["Mid"] = (8000, 15000), ["Senior"] = (15000, 28000), ["Lead"] = (28000, 45000) },
            ["Education"] = new() { ["Intern"] = (2000, 3500), ["Entry"] = (3500, 6000), ["Mid"] = (6000, 10000), ["Senior"] = (10000, 18000), ["Lead"] = (18000, 30000) },
            ["Design"] = new() { ["Intern"] = (2500, 4500), ["Entry"] = (4000, 8000), ["Mid"] = (8000, 14000), ["Senior"] = (14000, 24000), ["Lead"] = (24000, 40000) },
            ["Legal"] = new() { ["Intern"] = (3000, 5000), ["Entry"] = (5000, 10000), ["Mid"] = (10000, 18000), ["Senior"] = (18000, 35000), ["Lead"] = (35000, 60000) },
            ["Logistics"] = new() { ["Intern"] = (2500, 4000), ["Entry"] = (4000, 7000), ["Mid"] = (7000, 12000), ["Senior"] = (12000, 20000), ["Lead"] = (20000, 32000) },
            ["Management"] = new() { ["Intern"] = (3500, 6000), ["Entry"] = (6000, 10000), ["Mid"] = (10000, 18000), ["Senior"] = (18000, 30000), ["Lead"] = (30000, 55000) },
            ["Customer Support"] = new() { ["Intern"] = (2000, 3500), ["Entry"] = (3500, 6000), ["Mid"] = (6000, 10000), ["Senior"] = (10000, 16000), ["Lead"] = (16000, 25000) },
            ["Hospitality"] = new() { ["Intern"] = (1500, 3000), ["Entry"] = (2500, 5000), ["Mid"] = (5000, 9000), ["Senior"] = (9000, 15000), ["Lead"] = (15000, 25000) },
            ["Security"] = new() { ["Intern"] = (2000, 3500), ["Entry"] = (3000, 5500), ["Mid"] = (5500, 10000), ["Senior"] = (10000, 16000), ["Lead"] = (16000, 25000) },
            ["General"] = new() { ["Intern"] = (2000, 3500), ["Entry"] = (3500, 6000), ["Mid"] = (6000, 11000), ["Senior"] = (11000, 18000), ["Lead"] = (18000, 30000) },
        };

        private static readonly (string pattern, string category)[] RolePatterns =
        [
            (@"\b(data\s*(entry|clerk|typist)|admin(istrativ)?\s*(assistant|coordinator|support|clerk|specialist)?)\b", "Administrative"),
            (@"\b(account(ant|ing)?|audit(or)?|tax|treasury|payroll|finance|controller|cfo)\b", "Accounting"),
            (@"\b(sales\s*(rep|representative|associate|executive|engineer|consultant)?|account\s*manager|business\s*development|bde|bda)\b", "Sales"),
            (@"\b(marketing|brand|digital\s*m(arketing)?|social\s*media|seo|content|growth|pr|public\s*relations|communications|media)\b", "Marketing"),
            (@"\b(hr|human\s*resources|recruit(er|ing|ment)?|talent|people\s*(operations|partner)?|payroll|compensation|benefits)\b", "HR"),
            (@"\b(engineer(ing)?|developer|software|frontend|backend|full.?stack|web|mobile|app|ios|android|flutter|react|angular|vue|node|django|spring|api|devops|cloud|infrastructure|qa|quality\s*assurance|tester|test|automation|data\s*(scientist|analyst|engineer)?|machine\s*learning|ml|ai|sysadmin|it\s*support|network|security|cyber)\b", "Tech"),
            (@"\b(civil|architect(ure|ural)?|structural|mechanical|electrical|plumbing|mep|construction|site|survey(or|ing)?|draftsman|autocad|cad|bim|quantity\s*survey(or|ing)?)\b", "Engineering"),
            (@"\b(doctor|nurse|pharmac(ist|y)?|medical|clinical|healthcare|dentist|veterinary|lab\s*tech|radiolog(y|ist)?)\b", "Healthcare"),
            (@"\b(teacher|professor|instructor|lecturer|educator|academic|trainer|tutor|faculty)\b", "Education"),
            (@"\b(design(er)?|ui|ux|graphic|visual|creative|art\s*director|motion|animation|multimedia)\b", "Design"),
            (@"\b(lawyer|legal|attorney|paralegal|counsel|compliance|regulatory)\b", "Legal"),
            (@"\b(logistics|supply\s*chain|warehouse|inventory|procurement|purchasing|shipping|transport(ation)?|fleet)\b", "Logistics"),
            (@"\b(operation(s)?|project\s*manager|program\s*manager|product\s*(manager|owner)?|scrum\s*master|agile\s*coach|business\s*analyst)\b", "Management"),
            (@"\b(customer\s*(service|support|success|care)?|call\s*center|helpdesk|support\s*(engineer|specialist)?)\b", "Customer Support"),
            (@"\b(chef|cook|kitchen|barista|waiter|waitress|server|host|hospitality|hotel|restaurant|banquet|housekeeping|front\s*desk|concierge)\b", "Hospitality"),
            (@"\b(driver|delivery|courier|logistics\s*coordinator)\b", "Logistics"),
            (@"\b(security\s*(guard|officer)?|safety|hse|ehs|environmental)\b", "Security"),
        ];

        private static string ClassifyRole(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "General";
            var t = title.ToLower().Trim();
            foreach (var (pattern, category) in RolePatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(t, pattern))
                    return category;
            }
            return "General";
        }

        private static string DetectSeniority(string title, string experienceLevel)
        {
            if (string.IsNullOrWhiteSpace(title))
                return SeniorityOrder.Contains(experienceLevel) ? experienceLevel : "Mid";

            var t = title.ToLower().Trim();

            if (SeniorityOrder.Contains(experienceLevel))
            {
                foreach (var (level, keywords) in SeniorityKeywords)
                    foreach (var kw in keywords)
                        if (System.Text.RegularExpressions.Regex.IsMatch(t, $@"\b{System.Text.RegularExpressions.Regex.Escape(kw)}\b"))
                            return level;
                return experienceLevel;
            }

            foreach (var (level, keywords) in SeniorityKeywords)
                foreach (var kw in keywords)
                    if (System.Text.RegularExpressions.Regex.IsMatch(t, $@"\b{System.Text.RegularExpressions.Regex.Escape(kw)}\b"))
                        return level;

            return "Mid";
        }

        private static (decimal lo, decimal hi) ApplyVariation(string title, (decimal lo, decimal hi) band, long jobId)
        {
            var spread = band.hi - band.lo;
            var combinedSeed = (title.GetHashCode() * 1000L + jobId * 7) % 10000;
            if (combinedSeed < 0) combinedSeed = -combinedSeed;
            var variation = ((combinedSeed % 100) - 50) / 100.0 * (double)spread * 0.20;
            var mid = ((double)band.lo + (double)band.hi) / 2.0;
            var adjMid = mid + variation;
            var stretch = 0.6 + (combinedSeed % 40) / 100.0;
            var rangeWidth = Math.Max((double)spread * stretch, (double)spread * 0.35 + Math.Abs(variation) * 0.25);
            var newLo = Math.Max(adjMid - rangeWidth / 2.0, (double)band.lo * 0.8);
            var newHi = Math.Min(adjMid + rangeWidth / 2.0, (double)band.hi * 1.2);
            return ((decimal)Math.Round(newLo / 100) * 100, (decimal)Math.Round(newHi / 100) * 100);
        }

        public SalaryEstimate? GetEstimate(Job job)
        {
            if (job.SalaryMin.HasValue)
                return null;

            if (job.IsSalaryEstimated && job.EstimatedSalaryMin.HasValue)
            {
                return new SalaryEstimate
                {
                    EstimatedSalaryMin = job.EstimatedSalaryMin.Value,
                    EstimatedSalaryMax = job.EstimatedSalaryMax ?? job.EstimatedSalaryMin.Value
                };
            }

            var category = ClassifyRole(job.Title);
            var seniority = DetectSeniority(job.Title, job.ExperienceLevel);

            if (!SalaryBands.TryGetValue(category, out var bandByLevel))
                bandByLevel = SalaryBands["General"];

            if (!bandByLevel.TryGetValue(seniority, out var band))
                band = bandByLevel["Mid"];

            var varied = ApplyVariation(job.Title, band, job.Id);

            return new SalaryEstimate
            {
                EstimatedSalaryMin = varied.lo,
                EstimatedSalaryMax = varied.hi
            };
        }
    }
}

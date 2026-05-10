using Jobalatica.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jobalatica.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<UserSkill> UserSkills => Set<UserSkill>();
        public DbSet<DemandSnapshot> DemandSnapshots => Set<DemandSnapshot>();
        public DbSet<SalaryReport> SalaryReports => Set<SalaryReport>();
        public DbSet<SavedJob> SavedJobs => Set<SavedJob>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Job>().ToTable("Jobs");
            builder.Entity<Skill>().ToTable("Skills");
            builder.Entity<JobSkill>().ToTable("JobSkills");
            builder.Entity<UserSkill>().ToTable("UserSkills");
            builder.Entity<DemandSnapshot>().ToTable("DemandSnapshots");
            builder.Entity<SalaryReport>().ToTable("SalaryReports");
            builder.Entity<SavedJob>().ToTable("SavedJobs");

            builder.Entity<SavedJob>()
                .HasIndex(savedJob => new { savedJob.UserId, savedJob.JobId })
                .IsUnique();

            builder.Entity<UserSkill>()
                .HasIndex(userSkill => new { userSkill.UserId, userSkill.SkillId })
                .IsUnique();

            builder.Entity<Job>()
                .Property(job => job.SalaryMin)
                .HasPrecision(10, 2);

            builder.Entity<Job>()
                .Property(job => job.SalaryMax)
                .HasPrecision(10, 2);

            builder.Entity<DemandSnapshot>()
                .Property(snapshot => snapshot.AvgSalaryMin)
                .HasPrecision(10, 2);

            builder.Entity<DemandSnapshot>()
                .Property(snapshot => snapshot.AvgSalaryMax)
                .HasPrecision(10, 2);

            builder.Entity<SalaryReport>()
                .Property(report => report.Salary)
                .HasPrecision(10, 2);
        }
    }
}

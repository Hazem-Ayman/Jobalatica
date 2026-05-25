using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.Entities
{
    public class Skill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public int TotalJobMentions { get; set; }

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }
}

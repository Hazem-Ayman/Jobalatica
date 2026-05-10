namespace Jobalatica.Models.Entities
{
    public class UserSkill
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SkillId { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}

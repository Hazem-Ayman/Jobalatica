using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class RankingsViewModel
    {
        public List<(string Title, int Count)> TopRoles { get; set; } = new(); // Market's most posted roles
        public List<Skill> TopSkills { get; set; } = new(); // Market's most mentioned skills
    }
}

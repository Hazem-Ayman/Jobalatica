using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class RankingsViewModel
    {
        public List<(string Title, int Count)> TopRoles { get; set; } = new();
        public List<Skill> TopSkills { get; set; } = new();
    }
}

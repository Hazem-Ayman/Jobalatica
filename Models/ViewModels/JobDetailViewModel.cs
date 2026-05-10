using Jobalatica.Models.Entities;

namespace Jobalatica.Models.ViewModels
{
    public class JobDetailViewModel
    {
        public Job Job { get; set; } = null!;
        public bool IsSaved { get; set; }
        public List<Job> SimilarJobs { get; set; } = new();
    }
}

namespace Jobalatica.Models.Entities
{
    public class SavedJob
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public long JobId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public Job Job { get; set; } = null!;
    }
}

namespace Jobalatica.Models.Entities
{
    public class DemandSnapshot
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int PostingCount { get; set; }
        public decimal AvgSalaryMin { get; set; }
        public decimal AvgSalaryMax { get; set; }
        public DateOnly SnapshotDate { get; set; }
    }
}

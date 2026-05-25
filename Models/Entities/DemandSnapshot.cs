using System.ComponentModel.DataAnnotations;

namespace Jobalatica.Models.Entities
{
    public class DemandSnapshot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public int PostingCount { get; set; }
        public decimal AvgSalaryMin { get; set; }
        public decimal AvgSalaryMax { get; set; }
        public DateOnly SnapshotDate { get; set; }
    }
}

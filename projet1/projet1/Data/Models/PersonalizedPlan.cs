using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace projet1.Data.Models
{
    public class PersonalizedPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CoachSubscriptionId { get; set; }
        [ForeignKey("CoachSubscriptionId")]
        public CoachSubscription CoachSubscription { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public string FileType { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

    }
}

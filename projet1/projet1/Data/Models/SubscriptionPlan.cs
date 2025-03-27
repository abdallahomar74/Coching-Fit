using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace projet1.Data.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CoachId { get; set; }

        [ForeignKey("CoachId")]
        public ApplicationUser Coach { get; set; }

        [Required]
        public decimal Price { get; set; } // The subscription price

        [Required]
        public int DurationInDays { get; set; } // Subscription validity period (in days)

        public string Details { get; set; } // Additional subscription details

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

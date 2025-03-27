using System.ComponentModel.DataAnnotations;

namespace projet1.Models
{
    public class CreateSubscriptionPlanModel
    {
        [Required]
        public decimal Price { get; set; }

        [Required]
        public int DurationInDays { get; set; }

        public string Details { get; set; }
    }
}

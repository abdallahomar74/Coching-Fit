using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace projet1.Data.Models
{
    public class CoachSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubscriptionPlanId { get; set; }

        [ForeignKey("SubscriptionPlanId")]
        public SubscriptionPlan SubscriptionPlan { get; set; }

        [Required]
        public string SubscriberId { get; set; }

        [ForeignKey("SubscriberId")]
        public ApplicationUser Subscriber { get; set; }

        public DateTime SubscriptionDate { get; set; } = DateTime.UtcNow;


        public DateTime ExpirationDate { get; set; }
    }

}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace projet1.Data.Models
{
    public enum MessageType
    {
        Text,
        Audio
    }
    public class ChatMessage
    {
        [Key] public int Id { get; set; }

        [Required] public int CoachSubscriptionId { get; set; }
        [ForeignKey("CoachSubscriptionId")] public CoachSubscription Subscription { get; set; }

        [Required] public string SenderId { get; set; }
        [ForeignKey("SenderId")] public ApplicationUser Sender { get; set; }

        [Required] public MessageType Type { get; set; }
        public string? Text { get; set; }
        public string? AudioPath { get; set; }   
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

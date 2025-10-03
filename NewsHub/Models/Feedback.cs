using System.ComponentModel.DataAnnotations;

namespace NewsHub.Models
{
    public class Feedback
    {
        [Key]
        [Required]
        public Guid FeedbackId { get; set; }

        [Required]
        public required String Text { get; set; }
        public int Rating { get; set; }

        public required Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}

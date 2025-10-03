using System.ComponentModel.DataAnnotations;

namespace NewsHub.Models
{
    public enum ReasonType
    {
        Spam,
        Inappropriate,
        Harassment,
        Misinformation,
        Other,
        HateSpeech,
        Violence,
        Terrorism
    }
    public class Report
    {
        [Required]

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public required Guid ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        [Required, EnumDataType(typeof(ReasonType))]
        public required ReasonType Reason { get; set; }
        public required String Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

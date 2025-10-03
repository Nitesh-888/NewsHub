using System.ComponentModel.DataAnnotations;

namespace NewsHub.Models
{
    public enum ReactionType
    {
        Like,
        Dislike,
        Love,
        Angry,
        Sad,
        Wow,
        Funny
    }

    public class Reaction
    {
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        [Required]
        [EnumDataType(typeof(ReactionType))]
        public ReactionType Type { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

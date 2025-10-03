using NewsHub.Models;
namespace NewsHub.DTOs.Reactions
{
    public class ReactionResponseDTO
    {
        public required Guid ArticleId { get; set; }
        public required Guid UserId { get; set; }
        public required ReactionType Type { get; set; }
        public required DateTime ReactedAt { get; set; }
    }
}

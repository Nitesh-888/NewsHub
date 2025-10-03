using NewsHub.Models;
namespace NewsHub.DTOs.Reactions
{
    public class GetAllReactionsResponseDTO
    {
        public required Guid UserId { get; set; }
        public required ReactionType Type { get; set; }
        public required DateTime ReactedAt { get; set; }
    }
}

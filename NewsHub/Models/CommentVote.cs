namespace NewsHub.Models
{
    public class CommentVote
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public required int Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

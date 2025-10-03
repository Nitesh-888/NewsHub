namespace NewsHub.Models
{
    public class View
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    }
}

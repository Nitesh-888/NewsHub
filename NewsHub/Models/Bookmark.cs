using System.ComponentModel.DataAnnotations;

namespace NewsHub.Models
{
    public class Bookmark
    {
        [Required]

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = null!;
    }
}

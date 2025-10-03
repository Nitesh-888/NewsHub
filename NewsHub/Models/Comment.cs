using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsHub.Models
{
    public class Comment
    {
        [Key]
        public Guid CommentId { get; set; }
        [Required]
        public required String Text { get; set; }

        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<CommentVote> Votes { get; set; } = [];

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

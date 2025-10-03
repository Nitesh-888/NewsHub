using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsHub.Models
{
    public class User
    {
        [Key, Required]
        public Guid UserId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }

        public UserCred UserCred { get; set; } = null!;

        public ICollection<Article> Articles { get; set; } = [];

        public ICollection<Feedback> Feedbacks { get; set; } = [];

        public ICollection<Bookmark> Bookmarks { get; set; } = [];

        public ICollection<UserFollow> Followers { get; set; } = [];
        public ICollection<UserFollow> Followees { get; set; } = [];

        public ICollection<Report> Reports { get; set; } = [];

        public ICollection<Reaction> Reactions { get; set; } = [];

        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<View> Views { get; set; } = [];
        public ICollection<CommentVote> Votes { get; set; } = [];

        // Additional user details
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsHub.Models
{
    public class UserCred
    {
        [Key, Required, ForeignKey("User")]
        public Guid UserCredId { get; set; }
        public User User { get; set; } = new User();

        [Required]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string Password { get; set; } = null!;

        public int? Otp { get; set; }
        public DateTime? OtpGeneratedAt { get; set; }
    }
}

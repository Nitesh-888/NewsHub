using System.ComponentModel.DataAnnotations;

namespace NewsHub.Models
{
    public class TempUser
    {
        [Key]
        public required string Email { get; set; }
        public required string Username { get; set; }
        public string Password { get; set; } = null!;
        public required int Otp { get; set; }
        public DateTime OtpGeneratedAt { get; set; } = DateTime.UtcNow;
    }
}

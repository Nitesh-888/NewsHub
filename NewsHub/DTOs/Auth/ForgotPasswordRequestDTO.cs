using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Auth
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        public required string Email { get; set; }
    }
}

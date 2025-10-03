using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Auth
{
    public class LoginRequestDTO
    {
        public required string UsernameOrEmail { get; set; }

        public required string Password { get; set; }
    }
}

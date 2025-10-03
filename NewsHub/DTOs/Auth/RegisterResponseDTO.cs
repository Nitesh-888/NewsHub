using System.Security.Principal;

namespace NewsHub.DTOs.Auth
{
    public class RegisterResponseDTO
    {
        public required string Token { get; set; }
        public required string Message { get; set; }
    }
}

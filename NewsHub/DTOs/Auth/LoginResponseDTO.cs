namespace NewsHub.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public required string Token { get; set; }
        public required string Message { get; set; }
    }
}

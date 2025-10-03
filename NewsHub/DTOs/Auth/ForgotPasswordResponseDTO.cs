namespace NewsHub.DTOs.Auth
{
    public class ForgotPasswordResponseDTO
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string Message { get; set; }
    }
}

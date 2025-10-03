namespace NewsHub.DTOs.Auth
{
    public class ResetPasswordRequestDTO
    {
        public required string Email { get; set; }
        public required int Otp { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}

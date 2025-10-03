namespace NewsHub.DTOs.Users
{
    public class GetProfileResponseDTO
    {
        public required Guid UserId { get; set; }
        public required string? ProfileImageUrl { get; set; }
        public required string? Bio { get; set; }
        public required string? FirstName { get; set; }
        public required string? LastName { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required int FollowersCount { get; set; } = 0;
        public required int FolloweesCount { get; set; } = 0;
        public required int ArticlesCount { get; set; } = 0;
        public required int CommentsCount { get; set; } = 0;
        public required int ReactionsCount { get; set; } = 0;
        public required int BookmarksCount { get; set; } = 0;
        public required int ReportsCount { get; set; } = 0;
        public required int FeedbacksCount { get; set; } = 0;
        public required string? Country { get; set; }
        public required string? City { get; set; }
        public required string? Address { get; set; }
        public required string? PhoneNumber { get; set; }
        public required DateTime? DateOfBirth { get; set; }
        public required string? Gender { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}

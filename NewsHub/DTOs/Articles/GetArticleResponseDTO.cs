namespace NewsHub.DTOs.Articles
{
    public class GetArticleResponseDTO
    {
        public required Guid ArticleId { get; set; }
        public required string Title { get; set; }
        public required string? CoverImageUrl { get; set; }
        public required string Content { get; set; }

        public required string? AuthorName { get; set; }
        public required string? AuthorProfileImageUrl { get; set; }
        public required Guid AuthorId { get; set; }

        public required int ReactionsCount { get; set; }
        public required int CommentsCount { get; set; }
        public required int ViewsCount { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime LastUpdatedAt { get; set; }
        
    }
}

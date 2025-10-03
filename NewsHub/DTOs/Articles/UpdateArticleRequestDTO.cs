using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Articles
{
    public class UpdateArticleRequestDTO
    {
        public string? Title { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Content { get; set; }
    }
}

using NewsHub.Models;
using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Articles
{
    public class CreateArticleRequestDTO
    {
        [Required]
        public required string Title { get; set; }

        public string? CoverImageUrl { get; set; }

        [Required]
        public required string Content { get; set; }
    }
}

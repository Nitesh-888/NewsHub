using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Comments
{
    public class UpdateCommentRequestDTO
    {
        [Required]
        public required String Text { get; set; }
    }
}

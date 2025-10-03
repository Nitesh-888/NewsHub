using System.ComponentModel.DataAnnotations;

namespace NewsHub.DTOs.Comments
{
    public class CreateCommentRequestDTO
    {
        [Required]
        public required String Text { get; set; }
    }
}

using NewsHub.Models;
namespace NewsHub.DTOs.ArticleActions
{
    public class ReportRequestDTO
    {
        public required ReasonType Reason { get; set; }
        public required String Description { get; set; }
    }
}

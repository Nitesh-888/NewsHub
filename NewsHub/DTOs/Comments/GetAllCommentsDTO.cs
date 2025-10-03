namespace NewsHub.DTOs.Comments
{
    public class GetAllCommentsDTO
    {
        public required Guid CommentId { get; set; }
        public required String Text { get; set; }
        public required String AuthourId { get; set; }
        public required String? AuthorName { get; set; }
        public required DateTime CreatedAt { get; set; } 
    }
}

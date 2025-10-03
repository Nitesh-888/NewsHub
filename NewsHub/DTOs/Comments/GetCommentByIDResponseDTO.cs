namespace NewsHub.DTOs.Comments
{
    public class GetCommentByIDResponseDTO
    {
        public required Guid CommentId { get; set; }
        public required String Text { get; set; }
        public required String AuthourId { get; set; }
        public required String? AuthorName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required int VoteCount { get; set; }
        public required int UpvoteCount { get; set; }
        public required int DownvoteCount { get; set; }
        public required bool IsVotedByCurrentUser { get; set; }
        public required bool IsUpvotedByCurrentUser { get; set; }
    }
}

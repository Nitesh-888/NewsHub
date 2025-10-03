using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsHub.Data;
using NewsHub.Models;

namespace NewsHub.Controllers.Articles
{
    [Route("api/Articles/{ArticleId}/Comments/{CommentId}/Votes")]
    [ApiController]
    public class ArticleCommentVotesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ArticleCommentVotesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //UpVote for a comment
        [HttpPost, Authorize]
        public async Task<IActionResult> UpVoteForComment(Guid ArticleId, Guid CommentId)
        {
            //extract the userid from token
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = await _dbContext.Comments.FindAsync(CommentId);
            if (comment == null || comment.ArticleId != ArticleId)
            {
                return NotFound("Comment not found for the specified article.");
            }
            var existingVote = await _dbContext.CommentVotes.FindAsync(userId, CommentId);
            if (existingVote != null)
            {
                if (existingVote.Value == 1)
                {
                    _dbContext.CommentVotes.Remove(existingVote);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Vote removed successfully.");
                }
                else
                {
                    existingVote.Value = 1;
                    existingVote.CreatedAt = DateTime.UtcNow;
                    _dbContext.CommentVotes.Update(existingVote);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Vote updated successfully.");
                }
            }

            var vote = new CommentVote
            {
                UserId = userId,
                CommentId = CommentId,
                Value = 1,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.CommentVotes.Add(vote);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        //DownVote for a comment
        [HttpDelete, Authorize]
        public async Task<IActionResult> DownVoteForComment(Guid ArticleId, Guid CommentId)
        {
            //extract the userid from token
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = await _dbContext.Comments.FindAsync(CommentId);
            if (comment == null || comment.ArticleId != ArticleId)
            {
                return NotFound("Comment not found for the specified article.");
            }

            var existingVote = await _dbContext.CommentVotes.FindAsync(userId, CommentId);
            if (existingVote != null)
            {
                if(existingVote.Value == 1)
                {
                    //change the upvote to downvote
                    existingVote.Value = -1;
                    existingVote.CreatedAt = DateTime.UtcNow;
                    _dbContext.CommentVotes.Update(existingVote);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Vote updated successfully.");
                }
                else
                {
                    // remove the downvote
                    _dbContext.CommentVotes.Remove(existingVote);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Vote removed successfully.");
                }
            }

            var vote = new CommentVote
            {
                UserId = userId,
                CommentId = CommentId,
                Value = -1,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.CommentVotes.Add(vote);
            await _dbContext.SaveChangesAsync();

            return Ok("Vote added successfully.");
        }
    }
}

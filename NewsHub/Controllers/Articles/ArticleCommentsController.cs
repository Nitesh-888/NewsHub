using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.Data;
using NewsHub.DTOs.Comments;
using NewsHub.Models;
using System.Security.Claims;

namespace NewsHub.Controllers.Articles
{
    [Route("api/Articles/{ArticleId:guid}/Comments")]
    [ApiController]
    public class ArticleCommentsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ArticleCommentsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Get all Comments on article
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllComment(Guid ArticleId)
        {
            var article = await _dbContext.Articles
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);

            if (article == null)
            {
                return NotFound();
            }

            var response = article.Comments.Select(c => new GetAllCommentsDTO
            {
                CommentId = c.CommentId,
                Text = c.Text,
                AuthourId = c.UserId.ToString(),
                AuthorName = c.User.FirstName + " " + c.User.LastName,
                CreatedAt = c.CreatedAt,

            });

            return Ok(response);
        }

        //Get comment by id
        [HttpGet("{CommentId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetCommentById(Guid ArticleId, Guid CommentId)
        {
            //extract user id from token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)   
            {
                return Unauthorized();
            }
            var article = await _dbContext.Articles
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                    .ThenInclude(c => c.Votes)
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);

            if (article == null)
            {
                return NotFound();
            }

            var comment = article.Comments.SingleOrDefault(c => c.CommentId == CommentId);
            if (comment == null)
            {
                return NotFound();
            }

            var response = new GetCommentByIDResponseDTO
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                AuthourId = comment.UserId.ToString(),
                AuthorName = comment.User.FirstName + " " + comment.User.LastName,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                VoteCount = comment.Votes.Count,
                UpvoteCount = comment.Votes.Count(v => v.Value == 1),
                DownvoteCount = comment.Votes.Count(v => v.Value == -1),
                IsVotedByCurrentUser = comment.Votes.Any(v => v.UserId == userId),
                IsUpvotedByCurrentUser = comment.Votes.Any(v => v.UserId == userId && v.Value == 1)
            };

            return Ok(response);
        }

        //Create comment on article
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment(Guid ArticleId, [FromBody] CreateCommentRequestDTO CommentDto)
        {
            //extract user id from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }
            // extract article from db
            var article = await _dbContext.Articles
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);
            if (article == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Text = CommentDto.Text,
                ArticleId = ArticleId,
                UserId = userId,
                User = user,
                Article = article
            };

            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();

            var response = new GetAllCommentsDTO
            {
                CommentId = comment.CommentId,
                Text = comment.Text,
                AuthourId = comment.UserId.ToString(),
                AuthorName = user.FirstName + " " + user.LastName,
                CreatedAt = comment.CreatedAt,
            };

            return CreatedAtAction(nameof(GetCommentById), new { ArticleId = ArticleId, CommentId = comment.CommentId }, response);
        }

        //Update comment
        [HttpPut("{CommentId:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(Guid ArticleId, Guid CommentId, [FromBody] UpdateCommentRequestDTO CommentDto)
        {
            if (CommentDto == null)
            {
                return BadRequest();
            }

            //extract user id from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users
                .Include(u => u.Comments)
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if the comment belongs to the user
            if (!user.Comments.Any(c => c.CommentId == CommentId && c.ArticleId == ArticleId))
            {
                return Forbid();
            }

            var comment = await _dbContext.Comments.FindAsync(CommentId);
            if (comment == null || comment.ArticleId != ArticleId)
            {
                return NotFound();
            }

            comment.Text = CommentDto.Text;
            comment.UpdatedAt = DateTime.UtcNow;

            _dbContext.Comments.Update(comment);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        //Delete comment
        [HttpDelete("{CommentId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid ArticleId, Guid CommentId)
        {
            //extract user id from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = await _dbContext.Comments.FindAsync(CommentId);
            if (comment == null || comment.ArticleId != ArticleId)
            {
                return NotFound();
            }

            // Check if the comment belongs to the user
            if (comment.UserId != userId)
            {
                return Forbid();
            }

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.Data;
using NewsHub.DTOs.Reactions;
using NewsHub.Models;
using System.Security.Claims;

namespace NewsHub.Controllers.Articles
{
    [Route("api/Articles")]
    [ApiController]
    public class ArticleReactionsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ArticleReactionsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Get all reactions on article
        [HttpGet("{ArticleId:guid}/Reactions")]
        public async Task<IActionResult> GetAllReactions(Guid ArticleId)
        {
            //check if article exist or not
            var article = await _dbContext.Articles.FindAsync(ArticleId);
            if (article == null)
            {
                return BadRequest("Article not found.");
            }

            var reactions = await _dbContext.Reactions
                .Where(r => r.ArticleId == ArticleId)
                .Select(r => new GetAllReactionsResponseDTO
                {
                    UserId = r.UserId,
                    Type = r.Type,
                    ReactedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reactions);
        }
        // Get reaction by user on article
        [HttpGet("{ArticleId:guid}/Reaction"), Authorize]
        public async Task<IActionResult> GetReactionByUser(Guid ArticleId, Guid UserId)
        {
            //extract user id from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId) || userId != UserId)
            {
                return Unauthorized();
            }
            
            var reaction = await _dbContext.Reactions
                .SingleOrDefaultAsync(r => r.ArticleId == ArticleId && r.UserId == userId);
            if (reaction == null)
            {
                return NotFound();
            }
            var response = new GetAllReactionsResponseDTO
            {
                UserId = reaction.UserId,
                Type = reaction.Type,
                ReactedAt = reaction.CreatedAt
            };
            return Ok(response);
        }

        //user react to the article
        [Authorize]
        [HttpPost("{articleId:guid}/Reaction")]
        public async Task<IActionResult> ReactToArticle(Guid articleId, [FromBody] CreateReactionRequestDTO reactionDto)
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
                .Include(a => a.Reactions)
                .SingleOrDefaultAsync(a => a.ArticleId == articleId);
            if (article == null)
            {
                return NotFound();
            }

            // Check if article is already reacted by the user
            var existingReaction = article.Reactions.SingleOrDefault(r => r.UserId == userId);
            if (existingReaction != null)
            {
                existingReaction.Type = reactionDto.Type;
                _dbContext.Reactions.Update(existingReaction);
                await _dbContext.SaveChangesAsync();
                return Ok("Article reaction updated successfully.");
            }

            var reaction = new Reaction
            {
                ArticleId = articleId,
                UserId = userId,
                Type = reactionDto.Type
            };

            await _dbContext.Reactions.AddAsync(reaction);
            await _dbContext.SaveChangesAsync();

            return Ok("Article reacted successfully.");
        }

        //remove reaction
        [HttpDelete("{articleId:guid}/Reaction")]
        [Authorize]
        public async Task<IActionResult> RemoveReaction(Guid articleId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                return NotFound();
            }

            var existingReaction = await _dbContext.Reactions.SingleOrDefaultAsync(r => r.ArticleId == articleId && r.UserId == userId);
            if (existingReaction == null)
            {
                return BadRequest("You have not reacted to this article.");
            }

            _dbContext.Reactions.Remove(existingReaction);
            await _dbContext.SaveChangesAsync();

            return Ok("Article reaction removed successfully.");
        }
    }
}

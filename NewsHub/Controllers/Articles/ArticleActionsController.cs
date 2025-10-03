using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.DTOs.ArticleActions;
using NewsHub.Models;

namespace NewsHub.Controllers.Articles
{
    [Route("api/Articles/{ArticleId:guid}")]
    [ApiController]
    public class ArticleActionsController : ControllerBase
    {
        private readonly Data.AppDbContext _dbContext;
        public ArticleActionsController(Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Report an article
        [HttpPost("Report"), Authorize]
        public async Task<IActionResult> ReportArticle(Guid ArticleId, [FromBody] ReportRequestDTO request)
        {
            //exptract the userid from token
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

            var article = await _dbContext.Articles
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);
            if (article == null)
            {
                return NotFound("Article not found");
            }

            //check if the user has already reported this article
            var existingReport = await _dbContext.Reports
                .SingleOrDefaultAsync(r => r.ArticleId == ArticleId && r.UserId == userId);
            if (existingReport != null)
            {
                return BadRequest("You have already reported this article");
            }
            var report = new Report
            {
                ArticleId = ArticleId,
                Article = article,
                UserId = userId,
                User = user,
                Reason = request.Reason,
                Description = request.Description,
            };
            await _dbContext.Reports.AddAsync(report);
            await _dbContext.SaveChangesAsync();
            return Ok("Report submitted successfully");
        }

        //bookmark an article
        [HttpPost("Bookmark"), Authorize]
        public async Task<IActionResult> BookmarkArticle(Guid ArticleId)
        {
            //exptract the userid from token
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = _dbContext.Users
                .SingleOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var article = await _dbContext.Articles
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);
            if (article == null)
            {
                return NotFound("Article not found");
            }

            var existingBookmark =  await _dbContext.Bookmarks
                .SingleOrDefaultAsync(b => b.ArticleId == ArticleId && b.UserId == userId);
            if (existingBookmark != null)
            {
                _dbContext.Bookmarks.Remove(existingBookmark);
                await _dbContext.SaveChangesAsync();
                return Ok("Bookmark removed successfully");
            }

            var bookmark = new Bookmark
            {
                ArticleId = ArticleId,
                Article = article,
                UserId = userId,
                User = user,
            };
            await _dbContext.Bookmarks.AddAsync(bookmark);
            await _dbContext.SaveChangesAsync();
            return Ok("Article bookmarked successfully");
        }
    }
}

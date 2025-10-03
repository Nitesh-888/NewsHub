using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.Data;
using NewsHub.DTOs.Articles;
using NewsHub.Models;
using System.Security.Claims;

namespace NewsHub.Controllers.Articles
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ArticlesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Get all articles
        [HttpGet]
        public async Task<IActionResult> GetALLArticles()
        {
            var articlesList = await _dbContext.Articles.Select(a => new GetAllArticlesResponseDTO
            {
                ArticleId = a.ArticleId,
                Title = a.Title,
                CoverImageUrl = a.CoverImageUrl,
                ViewsCount = a.Views.Count,
                ReactionsCount = a.Reactions.Count,
                CommentsCount = a.Comments.Count,
                CreatedAt = a.CreatedAt,
            }).ToListAsync();
            return Ok(articlesList);
        }

        //Get article by id
        [HttpGet("{ArticleId:guid}"), Authorize]
        public async Task<IActionResult> GetArticleById(Guid ArticleId)
        {
            //extract the userid from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            User? user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            //extract the article from db
            var article = await _dbContext.Articles
                .Include(a => a.Reactions)
                .Include(a => a.Comments)
                .Include(a => a.Views)
                .SingleOrDefaultAsync(a => a.ArticleId == ArticleId);

            if (article == null)
            {
                return NotFound();
            }

            // extracting author from db
            var author = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == article.UserId);
            if (author == null)
            {
                return NotFound("Author not found");
            }

            // Record the view
            // Record a new view if the user is not the author and has not viewed the article before.
            bool isAuthor = article.UserId == userId;
            bool hasAlreadyViewed = article.Views.Any(v => v.UserId == userId);
            if (!isAuthor && !hasAlreadyViewed)
            {
                var view = new View
                {
                    ArticleId = article.ArticleId,
                    UserId = userId,
                };
                await _dbContext.Views.AddAsync(view);
                await _dbContext.SaveChangesAsync();
            }


            var response = new GetArticleResponseDTO
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                CoverImageUrl = article.CoverImageUrl,
                Content = article.Content,
                AuthorId = article.UserId,
                AuthorName = author.FirstName + " " + author.LastName,
                AuthorProfileImageUrl = author.ProfileImageUrl,

                ViewsCount = article.Views.Count,
                ReactionsCount = article.Reactions.Count,
                CommentsCount = article.Comments.Count,
                CreatedAt = article.CreatedAt,
                LastUpdatedAt = article.LastUpdatedAt
            };
            return Ok(response);
        }

        //Create article
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequestDTO ArticleDto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }
            User? user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }
            var article = new Article
            {
                Title = ArticleDto.Title,
                Content = ArticleDto.Content,
                CoverImageUrl = ArticleDto.CoverImageUrl,
                UserId = userId,
                User = user
            };

            await _dbContext.Articles.AddAsync(article);
            await _dbContext.SaveChangesAsync();

            var response = new GetArticleResponseDTO
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                CoverImageUrl = article.CoverImageUrl,
                Content = article.Content,
                AuthorId = article.UserId,
                AuthorName = user.FirstName + " " + user.LastName,
                AuthorProfileImageUrl = user.ProfileImageUrl,
                ViewsCount = article.Views.Count,
                ReactionsCount = article.Reactions.Count,
                CommentsCount = article.Comments.Count,
                CreatedAt = article.CreatedAt,
                LastUpdatedAt = article.LastUpdatedAt
            };
            return CreatedAtAction(nameof(GetArticleById), new { article.ArticleId }, response);
        }

        //Update article
        [Authorize]
        [HttpPut("{articleId:guid}")]
        public async Task<IActionResult> UpdateArticle(Guid articleId, [FromBody] UpdateArticleRequestDTO ArticleDto)
        {
            if (ArticleDto == null)
            {
                return BadRequest();
            }
            var UserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (UserIdString == null || !Guid.TryParse(UserIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users
                .Include(u => u.Articles)
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if the article belongs to the user
            if (!user.Articles.Any(a => a.ArticleId == articleId))
            {
                return Forbid();
            }

            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                return NotFound();
            }

            article.Title = ArticleDto.Title ?? article.Title;
            article.Content = ArticleDto.Content ?? article.Content;
            article.CoverImageUrl = ArticleDto.CoverImageUrl ?? article.CoverImageUrl;
            article.LastUpdatedAt = DateTime.UtcNow;

            _dbContext.Articles.Update(article);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        //Delete article
        [Authorize]
        [HttpDelete("{articleId:guid}")]
        public async Task<IActionResult> DeleteArticle(Guid articleId)
        {
            var UserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (UserIdString == null || !Guid.TryParse(UserIdString, out Guid userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                return NotFound();
            }

            // Check if the article belongs to the user
            if (article.UserId != userId)
            {
                return Forbid();
            }


            _dbContext.Articles.Remove(article);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

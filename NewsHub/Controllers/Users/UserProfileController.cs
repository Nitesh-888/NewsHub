using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.Data;
using NewsHub.DTOs.Users;

namespace NewsHub.Controllers.Users
{
    [Route("api/User")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public UserProfileController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users
                .Include(u => u.UserCred)
                .Include(u => u.Followers)
                .Include(u => u.Followees)
                .Include(u => u.Articles)
                .Include(u => u.Reactions)
                .Include(u => u.Bookmarks)
                .Include(u => u.Feedbacks)
                .Include(u => u.Comments)
                .Include(u => u.Reports)
                .SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }
            var response = new GetProfileResponseDTO
            {
                UserId = user.UserId,
                ProfileImageUrl = user.ProfileImageUrl,
                Bio = user.Bio,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.UserCred.Email,
                Username = user.UserCred.Username,
                FollowersCount = user.Followers.Count,
                FolloweesCount = user.Followees.Count,
                ArticlesCount = user.Articles.Count,
                CommentsCount = user.Comments.Count,
                ReactionsCount = user.Reactions.Count,
                BookmarksCount = user.Bookmarks.Count,
                ReportsCount = user.Reports.Count,
                FeedbacksCount = user.Feedbacks.Count,
                Country = user.Country,
                City = user.City,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                CreatedAt = user.CreatedAt
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDTO updatedInfo)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.Include(u => u.UserCred).SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            //checking for unique email
            if (updatedInfo.Email != null && updatedInfo.Email != user.UserCred.Email)
            {
                var emailExists = await _dbContext.UserCreds.AnyAsync(u => u.Email == updatedInfo.Email);
                if (emailExists)
                {
                    return BadRequest("Email already exists");
                }
            }

            //checking for unique username
            if (updatedInfo.Username != null && updatedInfo.Username != user.UserCred.Username)
            {
                var usernameExists = await _dbContext.UserCreds.AnyAsync(u => u.Username == updatedInfo.Username);
                if (usernameExists)
                {
                    return BadRequest("Username already exists");
                }
            }

            user.ProfileImageUrl = updatedInfo.ProfileImageUrl;
            user.Bio = updatedInfo.Bio;
            user.FirstName = updatedInfo.FirstName ?? user.FirstName;
            user.LastName = updatedInfo.LastName ?? user.LastName;
            user.UserCred.Email = updatedInfo.Email ?? user.UserCred.Email;
            user.UserCred.Username = updatedInfo.Username ?? user.UserCred.Username;
            user.Country = updatedInfo.Country ?? user.Country;
            user.City = updatedInfo.City ?? user.City;
            user.Address = updatedInfo.Address ?? user.Address;
            user.PhoneNumber = updatedInfo.PhoneNumber ?? user.PhoneNumber;
            user.DateOfBirth = updatedInfo.DateOfBirth ?? user.DateOfBirth;
            user.Gender = updatedInfo.Gender ?? user.Gender;
            user.UpdatedAt = DateTime.UtcNow;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

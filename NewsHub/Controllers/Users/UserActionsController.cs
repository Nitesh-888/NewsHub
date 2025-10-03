using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsHub.Data;
using System.Security.Claims;
using NewsHub.DTOs.Users;
using NewsHub.Models;

namespace NewsHub.Controllers.Users
{
    [Route("api/User")]
    [ApiController]
    public class UserActionsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public UserActionsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Feedback
        [Authorize]
        [HttpPost("Feedback")]
        public async Task<IActionResult> GiveFeedback([FromBody] FeedbackDTO feedback)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }
            var newFeedback = new Feedback
            {
                Text = feedback.Text,
                Rating = feedback.Rating,
                UserId = userId,
                User = user
            };

            _dbContext.Feedbacks.Add(newFeedback);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        //Delete User
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            //extract user id from token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users
                .Include(u => u.UserCred)
                .Include(u => u.Articles)
                .Include(u => u.Comments)
                .Include(u => u.Reactions)
                .Include(u => u.Bookmarks)
                .Include(u => u.Feedbacks)
                .Include(u => u.Followers)
                .Include(u => u.Followees)
                .Include(u => u.Reports)
                .Include(u => u.Views)
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove related entities first due to foreign key constraints
            _dbContext.Comments.RemoveRange(user.Comments);
            _dbContext.Reactions.RemoveRange(user.Reactions);
            _dbContext.Bookmarks.RemoveRange(user.Bookmarks);
            _dbContext.Feedbacks.RemoveRange(user.Feedbacks);
            _dbContext.UserFollows.RemoveRange(user.Followers);
            _dbContext.UserFollows.RemoveRange(user.Followees);
            _dbContext.Reports.RemoveRange(user.Reports);
            _dbContext.Articles.RemoveRange(user.Articles);
            _dbContext.UserCreds.Remove(user.UserCred);

            // Finally, remove the user
            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        //follow another user
        [Authorize]
        [HttpPost("Follow/{followeeId}")]
        public async Task<IActionResult> FollowUser(Guid followeeId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            var followee = await _dbContext.Users.FindAsync(followeeId);
            if (user == null || followee == null)
            {
                return NotFound();
            }
            if (userId == followeeId)
            {
                return BadRequest("You cannot follow yourself.");
            }
            if (await _dbContext.UserFollows.AnyAsync(uf => uf.FollowerId == userId && uf.FolloweeId == followeeId))
            {
                return BadRequest("You are already following this user.");
            }

            var userFollow = new UserFollow
            {
                FollowerId = userId,
                Follower = user,
                FolloweeId = followeeId,
                Followee = followee
            };

            await _dbContext.UserFollows.AddAsync(userFollow);
            await _dbContext.SaveChangesAsync();

            return Ok("Followed successfully.");
        }

        //unfollow another user
        [Authorize]
        [HttpPost("Unfollow/{followeeId}")]
        public async Task<IActionResult> UnfollowUser(Guid followeeId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var userFollow = await _dbContext.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == userId && uf.FolloweeId == followeeId);
            if (userFollow == null)
            {
                return NotFound("You are not following this user.");
            }

            _dbContext.UserFollows.Remove(userFollow);
            await _dbContext.SaveChangesAsync();

            return Ok("Unfollowed successfully.");
        }
    }
}

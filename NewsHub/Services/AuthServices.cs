using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NewsHub.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsHub.Services
{
    public class AuthServices
    {
        private readonly IConfiguration _config;
        public AuthServices(IConfiguration config)
        {
            _config = config;
        }
        public Random random = new();
        public string GenerateJwtToken(string UserId)
        {
            var jwt = _config.GetSection("Jwt");
            var jwtKey = jwt["Key"];
            var expiresTime = jwt["DurationInMinutes"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            if (string.IsNullOrEmpty(expiresTime) || !double.TryParse(expiresTime, out _))
            {
                throw new InvalidOperationException("JWT Expiration time is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var signingKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, UserId)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                signingCredentials: signingKey,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(expiresTime))
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //generate jwt token for email verification and password reset
        public string GenerateJwtForEmailVerification(string email, int expireMinutes = 10)
        {
            var jwt = _config.GetSection("Jwt");
            var jwtKey = jwt["Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var signingKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                signingCredentials: signingKey,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsHub.Data;
using NewsHub.Models;
using System;
using NewsHub.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using NewsHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NewsHub.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthServices _authServices;
        private readonly MailServices _mailServices;
        private readonly PasswordHasher<TempUser> _tempUserPasswordHasher = new();
        private readonly PasswordHasher<UserCred> _userPasswordHasher = new();

        public AuthController(AppDbContext dbContext, AuthServices AuthServices, MailServices mailServices)
        {
            _dbContext = dbContext;
            _authServices = AuthServices;
            _mailServices = mailServices;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequestDTO userReq)
        {
            if (userReq == null || userReq.Email == null || userReq.Username == null || userReq.Password == null)
            {
                return BadRequest();
            }
            //unique username
            var userName = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Username == userReq.Username);
            if (userName != null)
            {
                return BadRequest("Username already exists");
            }

            //unique email
            var email = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Email == userReq.Email);
            if (email != null)
            {
                return BadRequest("Email already exists");
            }

            //check if temp user exists with same email or username, if yes delete
            var tempUserExists = await _dbContext.TempUsers.FindAsync(userReq.Email);
            if (tempUserExists != null)
            {
                //check otp resend time 1 minute
                if((DateTime.UtcNow - tempUserExists.OtpGeneratedAt).TotalMinutes < 1)
                {
                    return BadRequest("OTP already sent. Please wait before requesting again.");
                }
                _dbContext.TempUsers.Remove(tempUserExists);
                await _dbContext.SaveChangesAsync();
            }
            var tempUsernameExists = await _dbContext.TempUsers.SingleOrDefaultAsync(u => u.Username == userReq.Username);
            if (tempUsernameExists != null)
            {
                _dbContext.TempUsers.Remove(tempUsernameExists);
                await _dbContext.SaveChangesAsync();
            }


            //generate otp
            var otp = _authServices.random.Next(100000, 1000000);

            //send otp email
            await _mailServices.SendOtpEmail(userReq.Email, userReq.Username, otp.ToString());

            //store temp user
            var tempUser = new TempUser()
            {
                Email = userReq.Email,
                Username = userReq.Username,
                Otp = otp,
                OtpGeneratedAt = DateTime.UtcNow
            };
            tempUser.Password = _tempUserPasswordHasher.HashPassword(tempUser, userReq.Password);

            await _dbContext.TempUsers.AddAsync(tempUser);
            await _dbContext.SaveChangesAsync();

            //jwt for otp verification
            var jwtToken = _authServices.GenerateJwtForEmailVerification(userReq.Email, 10);

            var response = new RegisterResponseDTO()
            {
                Token = jwtToken,
                Message = "OTP sent to email. Please verify to complete registration."
            };
            return Ok(response);
        }

        //verify email and otp
        [HttpPost("Register/Verify"), Authorize]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOptRequestDTO verifyReq)
        {
            //get email from token
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return Unauthorized();
            }

            var tempUser = _dbContext.TempUsers.Find(email);
            if (tempUser == null)
            {
                return BadRequest("Invalid request");
            }
            //check otp expiry 10 minutes
            if ((DateTime.UtcNow - tempUser.OtpGeneratedAt).TotalMinutes > 10)
            {
                _dbContext.TempUsers.Remove(tempUser);
                await _dbContext.SaveChangesAsync();
                return BadRequest("OTP expired. Please request a new one.");
            }

            //check otp
            if (tempUser.Otp != verifyReq.Otp)
            {
                return BadRequest("Invalid OTP");
            }

            //create user
            var newUser = new UserCred()
            {
                Email = tempUser.Email,
                Username = tempUser.Username,
                Password = tempUser.Password
            };
            //remove temp user
            _dbContext.TempUsers.Remove(tempUser);

            await _dbContext.UserCreds.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            //jwt token
            var jwtToken = _authServices.GenerateJwtToken(newUser.UserCredId.ToString());

            //response 
            var response = new RegisterResponseDTO()
            {
                Token = jwtToken,
                Message = "User registered successfully"
            };
            return Ok(response);

        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequestDTO userReq)
        {
            if (userReq == null || userReq.Password == null || userReq.UsernameOrEmail == null)
            {
                return BadRequest();
            }
            var userCred = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Username == userReq.UsernameOrEmail || u.Email == userReq.UsernameOrEmail);
            if (userCred == null)
            {
                return BadRequest("Invalid username or password");
            }

            var result = _userPasswordHasher.VerifyHashedPassword(userCred, userCred.Password, userReq.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid username or password");
            }
            var jwtToken = _authServices.GenerateJwtToken(userCred.UserCredId.ToString());
            var response = new LoginResponseDTO()
            {
                Token = jwtToken,
                Message = "User logged in successfully"
            };
            return Ok(response);
        }

        //forgot password
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO forgotPasswordRequest)
        {
            if (forgotPasswordRequest == null || string.IsNullOrEmpty(forgotPasswordRequest.Email))
            {
                return BadRequest("Invalid request");
            }

            var user = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Email == forgotPasswordRequest.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            //check otp resend time 1 minute
            if (user.OtpGeneratedAt != null && (DateTime.UtcNow - user.OtpGeneratedAt.Value).TotalMinutes < 1)
            {
                return BadRequest("OTP already sent. Please wait before requesting again.");
            }

            // Generate OTP and send email
            var otp = _authServices.random.Next(100000, 1000000);
            await _mailServices.SendOtpEmail(user.Email, user.Username, otp.ToString());

            // Save OTP to database
            user.Otp = otp;
            user.OtpGeneratedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Generate JWT token
            var jwtToken = _authServices.GenerateJwtForEmailVerification(user.Email, 10);
            var response = new ForgotPasswordResponseDTO()
            {
                Email = user.Email,
                Token = jwtToken,
                Message = "OTP sent to email. Please verify to reset your password."
            };
            return Ok(response);
        }

        //reset password
        [HttpPost("ResetPassword"), Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO resetPasswordRequest)
        {
            //get email from token
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return Unauthorized();
            }
            var user = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            //check otp expiry 10 minutes
            if (user.Otp == null || user.OtpGeneratedAt == null || (DateTime.UtcNow - user.OtpGeneratedAt.Value).TotalMinutes > 10)
            {
                user.Otp = null;
                user.OtpGeneratedAt = null;
                await _dbContext.SaveChangesAsync();
                return BadRequest("OTP expired. Please request a new one.");
            }

            //check otp
            if (user.Otp != resetPasswordRequest.Otp)
            {
                return BadRequest("Invalid OTP");
            }
            //check new password and confirm password match
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmPassword)
            {
                return BadRequest("New password and confirm password do not match");
            }

            user.Password = _userPasswordHasher.HashPassword(user, resetPasswordRequest.NewPassword);
            user.Otp = null;
            user.OtpGeneratedAt = null;
            await _dbContext.SaveChangesAsync();
            return Ok("Password reset successfully");
        }

        //resend otp
        [HttpGet("ResendOtp"), Authorize]
        public async Task<IActionResult> ResendOtp()
        {
            //get email from token
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return Unauthorized();
            }

            //Resend otp for forgot password 
            var user = await _dbContext.UserCreds.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null && user.OtpGeneratedAt != null)
            {
                if ((DateTime.UtcNow - user.OtpGeneratedAt.Value).TotalMinutes < 1)
                {
                    return BadRequest("OTP already sent. Please wait before requesting again.");
                }
                //generate new otp
                var otpForgotPass = _authServices.random.Next(100000, 1000000);
                //send otp email
                await _mailServices.SendOtpEmail(user.Email, user.Username, otpForgotPass.ToString());
                //update otp and otp generated time
                user.Otp = otpForgotPass;
                user.OtpGeneratedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return Ok("OTP resent successfully");
            }else if(user != null && user.OtpGeneratedAt == null)
            {
                return BadRequest("No OTP request found. Please initiate forgot password process.");
            }

            //Resend otp for registration
            var tempUser = await _dbContext.TempUsers.SingleOrDefaultAsync(u => u.Email == email);
            if (tempUser == null)
            {
                return BadRequest("Invalid request");
            }
            //check otp resend time 1 minute
            if ((DateTime.UtcNow - tempUser.OtpGeneratedAt).TotalMinutes < 1)
            {
                return BadRequest("OTP already sent. Please wait before requesting again.");
            }

            //generate new otp
            var otp = _authServices.random.Next(100000, 1000000);

            //send otp email
            await _mailServices.SendOtpEmail(tempUser.Email, tempUser.Username, otp.ToString());

            //update otp and otp generated time
            tempUser.Otp = otp;
            tempUser.OtpGeneratedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok("OTP resent successfully");
        }
    }
}

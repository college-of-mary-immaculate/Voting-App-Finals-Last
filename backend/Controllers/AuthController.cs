// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingSystemBackend.Models;
using VotingSystemBackend.Services;

namespace VotingSystemBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;

        public AuthController(
            IAuthService authService,
            IEmailService emailService,
            IAuditService auditService)
        {
            _authService = authService;
            _emailService = emailService;
            _auditService = auditService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(result.User.Email, result.ConfirmationToken);

            await _auditService.LogActionAsync("User Registered", "User", result.User.Id, Request);

            return Ok(new
            {
                message = "Registration successful. Please check your email to confirm your account.",
                userId = result.User.Id
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model, Request.HttpContext.Connection.RemoteIpAddress?.ToString());

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            await _auditService.LogActionAsync("User Logged In", "User", result.User.Id, Request);

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = new
                {
                    result.User.Id,
                    result.User.FirstName,
                    result.User.LastName,
                    result.User.Email,
                    result.User.VoterId,
                    result.User.IsEmailConfirmed,
                    result.User.IsTwoFactorEnabled,
                    roles = result.Roles
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var result = await _authService.RefreshTokenAsync(model.RefreshToken);
            
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.NewRefreshToken,
                expiresAt = result.ExpiresAt
            });
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.RevokeTokenAsync(userId, model.RefreshToken);
            
            if (!result)
                return BadRequest(new { message = "Failed to revoke token" });

            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Email Confirmed", "User", userId, Request);

            return Ok(new { message = "Email confirmed successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var result = await _authService.GeneratePasswordResetTokenAsync(model.Email);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _emailService.SendPasswordResetEmailAsync(model.Email, result.Token);

            return Ok(new { message = "Password reset email sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var result = await _authService.ResetPasswordAsync(model);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Password Reset", "User", result.UserId, Request);

            return Ok(new { message = "Password reset successfully" });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _authService.GetUserProfileAsync(userId);
            
            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _authService.UpdateProfileAsync(userId, model);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Profile Updated", "User", userId, Request);

            return Ok(new { message = "Profile updated successfully", profile = result.Profile });
        }
    }
}
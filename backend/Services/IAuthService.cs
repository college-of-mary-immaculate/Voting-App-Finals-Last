using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterModel model);
        Task<LoginResult> LoginAsync(LoginModel model, string ipAddress);
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string userId, string refreshToken);
        Task<AuthResult> ConfirmEmailAsync(string userId, string token);
        Task<PasswordResetResult> GeneratePasswordResetTokenAsync(string email);
        Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordModel model);
        Task<UserProfileModel> GetUserProfileAsync(string userId);
        Task<UpdateProfileResult> UpdateProfileAsync(string userId, UpdateProfileModel model);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public string ConfirmationToken { get; set; }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class RefreshTokenResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string NewRefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class PasswordResetResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }

    public class ResetPasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateProfileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserProfileModel Profile { get; set; }
    }
}
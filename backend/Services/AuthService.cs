using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VotingSystemBackend.Data;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User with this email already exists"
                };
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                VoterId = GenerateVoterId(),
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Add to Voter role by default
            if (!await _roleManager.RoleExistsAsync("Voter"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Voter"));
            }
            await _userManager.AddToRoleAsync(user, "Voter");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return new AuthResult
            {
                Success = true,
                User = user,
                ConfirmationToken = token
            };
        }

        public async Task<LoginResult> LoginAsync(LoginModel model, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            if (!user.IsActive)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Account is deactivated. Please contact administrator."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (result.IsLockedOut)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Account locked out. Please try again later."
                };
            }

            if (!result.Succeeded)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var roles = await _userManager.GetRolesAsync(user);

            // Save refresh token
            user.UserRefreshTokens ??= new List<UserRefreshToken>();
            user.UserRefreshTokens.Add(new UserRefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            await _userManager.UpdateAsync(user);

            return new LoginResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user,
                Roles = roles.ToList()
            };
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRefreshTokens)
                .FirstOrDefaultAsync(u => u.UserRefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            var token = user.UserRefreshTokens.First(t => t.Token == refreshToken);

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Refresh token expired"
                };
            }

            var newJwtToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Mark old token as used
            token.IsUsed = true;
            token.RevokedAt = DateTime.UtcNow;

            // Add new refresh token
            user.UserRefreshTokens.Add(new UserRefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _userManager.UpdateAsync(user);

            return new RefreshTokenResult
            {
                Success = true,
                Token = newJwtToken,
                NewRefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> RevokeTokenAsync(string userId, string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var token = user.UserRefreshTokens?.FirstOrDefault(t => t.Token == refreshToken);

            if (token == null)
                return false;

            token.RevokedAt = DateTime.UtcNow;
            token.IsUsed = true;

            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<AuthResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email confirmation failed"
                };
            }

            user.IsEmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                Success = true,
                User = user
            };
        }

        public async Task<PasswordResetResult> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                return new PasswordResetResult
                {
                    Success = false,
                    Message = "If the email exists, a reset link will be sent"
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new PasswordResetResult
            {
                Success = true,
                Token = token
            };
        }

        public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Message = "Password reset failed"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                return new ResetPasswordResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new ResetPasswordResult
            {
                Success = true,
                UserId = user.Id
            };
        }

        public async Task<UserProfileModel> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                VoterId = user.VoterId,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                IsEmailConfirmed = user.IsEmailConfirmed,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };
        }

        public async Task<UpdateProfileResult> UpdateProfileAsync(string userId, UpdateProfileModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return new UpdateProfileResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Address = model.Address ?? user.Address;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new UpdateProfileResult
                {
                    Success = false,
                    Message = "Failed to update profile"
                };
            }

            var profile = await GetUserProfileAsync(userId);

            return new UpdateProfileResult
            {
                Success = true,
                Profile = profile
            };
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("voterId", user.VoterId)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateVoterId()
        {
            return $"VOT{DateTime.UtcNow.Ticks.ToString().Substring(0, 8)}{new Random().Next(1000, 9999)}";
        }
    }
}
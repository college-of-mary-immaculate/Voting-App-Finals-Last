using System.ComponentModel.DataAnnotations;

namespace VotingSystemBackend.Models
{
    public class RegisterModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RefreshTokenModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class RevokeTokenModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateProfileModel
    {
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        public string Address { get; set; }

        public string ProfilePictureUrl { get; set; }
    }

    public class CreateElectionModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string ElectionType { get; set; }

        public int MaxVotesPerVoter { get; set; } = 1;

        public bool IsPublic { get; set; } = true;

        public string ElectionRules { get; set; }
    }

    public class UpdateElectionModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ElectionType { get; set; }
        public int? MaxVotesPerVoter { get; set; }
        public bool? IsPublic { get; set; }
        public string ElectionRules { get; set; }
    }

    public class CreateCandidateModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string Biography { get; set; }

        public string PhotoUrl { get; set; }

        public string Party { get; set; }

        [Required]
        public string Position { get; set; }

        public string Manifesto { get; set; }

        public string CampaignSlogan { get; set; }

        public string Website { get; set; }

        public string SocialMediaLinks { get; set; }
    }

    public class UpdateCandidateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Biography { get; set; }
        public string PhotoUrl { get; set; }
        public string Party { get; set; }
        public string Position { get; set; }
        public string Manifesto { get; set; }
        public string CampaignSlogan { get; set; }
        public string Website { get; set; }
        public string SocialMediaLinks { get; set; }
        public bool? IsApproved { get; set; }
    }

    public class CastVoteModel
    {
        [Required]
        public int ElectionId { get; set; }

        [Required]
        public int CandidateId { get; set; }
    }

    public class UserProfileModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string VoterId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; }
    }
}
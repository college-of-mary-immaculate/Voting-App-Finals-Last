// Models/User.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VotingSystemBackend.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string VoterId { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string ProfilePictureUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<Vote> Votes { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
    }

    public class Election
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ElectionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ElectionType { get; set; }
        public int MaxVotesPerVoter { get; set; } = 1;
        public bool IsPublic { get; set; } = true;
        public string ElectionRules { get; set; }
        public int TotalRegisteredVoters { get; set; }
        public int TotalVotesCast { get; set; }
        
        // Navigation properties
        public virtual ICollection<Candidate> Candidates { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
        public virtual ICollection<ElectionAudit> Audits { get; set; }
    }

    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Biography { get; set; }
        public string PhotoUrl { get; set; }
        public string Party { get; set; }
        public string Position { get; set; }
        public string Manifesto { get; set; }
        public string CampaignSlogan { get; set; }
        public string Website { get; set; }
        public string SocialMediaLinks { get; set; }
        public bool IsApproved { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int ElectionId { get; set; }
        public virtual Election Election { get; set; }
        
        // Navigation properties
        public virtual ICollection<Vote> Votes { get; set; }
    }

    public class Vote
    {
        [Key]
        public int Id { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string TransactionHash { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }
        
        // Foreign keys
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        public int ElectionId { get; set; }
        public virtual Election Election { get; set; }
        
        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public string ActionUrl { get; set; }
        
        // Foreign key
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Changes { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class ElectionAudit
    {
        [Key]
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public virtual Election Election { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string Details { get; set; }
    }

    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool IsEncrypted { get; set; } = false;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class WebhookConfiguration
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Secret { get; set; }
        public string Events { get; set; } // Comma-separated events
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int RetryCount { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
    }

    public enum ElectionStatus
    {
        Draft,
        Scheduled,
        Active,
        Paused,
        Completed,
        Cancelled
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        ElectionReminder,
        VoteConfirmation
    }
}
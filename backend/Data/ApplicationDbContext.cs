// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Election> Elections { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ElectionAudit> ElectionAudits { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<WebhookConfiguration> WebhookConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and constraints
            builder.Entity<Vote>()
                .HasIndex(v => new { v.UserId, v.ElectionId })
                .IsUnique();

            builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vote>()
                .HasOne(v => v.Election)
                .WithMany(e => e.Votes)
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vote>()
                .HasOne(v => v.Candidate)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Candidate>()
                .HasOne(c => c.Election)
                .WithMany(e => e.Candidates)
                .HasForeignKey(c => c.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.Entity<Election>()
                .HasIndex(e => e.Status);

            builder.Entity<Election>()
                .HasIndex(e => e.StartDate);

            builder.Entity<Election>()
                .HasIndex(e => e.EndDate);

            builder.Entity<Vote>()
                .HasIndex(v => v.VotedAt);

            builder.Entity<Vote>()
                .HasIndex(v => v.TransactionHash)
                .IsUnique();

            builder.Entity<Candidate>()
                .HasIndex(c => c.ElectionId);

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            builder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);
        }
    }
}
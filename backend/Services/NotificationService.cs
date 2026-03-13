using System.Linq;
using Microsoft.EntityFrameworkCore;
using VotingSystemBackend.Data;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendElectionStartedNotificationAsync(int electionId)
        {
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null) return;

            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var notifications = users.Select(u => new Notification
            {
                UserId = u.Id,
                Title = "Election Started",
                Message = $"The election '{election.Title}' has started. Cast your vote now.",
                Type = NotificationType.ElectionReminder,
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task SendElectionEndedNotificationAsync(int electionId)
        {
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null) return;

            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var notifications = users.Select(u => new Notification
            {
                UserId = u.Id,
                Title = "Election Ended",
                Message = $"The election '{election.Title}' has ended. Check the results.",
                Type = NotificationType.ElectionReminder,
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task SendVoteConfirmationAsync(string userId, Vote vote)
        {
            if (string.IsNullOrEmpty(userId) || vote == null)
                return;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return;

            var candidate = await _context.Candidates.FindAsync(vote.CandidateId);

            var message = candidate != null
                ? $"Your vote for {candidate.FirstName} {candidate.LastName} has been recorded."
                : "Your vote has been recorded.";

            var notification = new Notification
            {
                UserId = userId,
                Title = "Vote Confirmed",
                Message = message,
                Type = NotificationType.VoteConfirmation,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}

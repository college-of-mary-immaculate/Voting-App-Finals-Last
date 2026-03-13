using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public interface INotificationService
    {
        Task SendElectionStartedNotificationAsync(int electionId);
        Task SendElectionEndedNotificationAsync(int electionId);
        Task SendVoteConfirmationAsync(string userId, Vote vote);
    }
}

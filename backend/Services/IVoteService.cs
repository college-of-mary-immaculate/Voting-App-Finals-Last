using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public interface IVoteService
    {
        Task<VoteResult> CastVoteAsync(CastVoteModel model, string userId, string ipAddress, string userAgent);
        Task<IEnumerable<Vote>> GetUserVotesAsync(string userId);
        Task<IEnumerable<Vote>> GetElectionVotesAsync(int electionId, int page, int pageSize);
        Task<Vote> VerifyVoteAsync(int voteId);
        Task<int> GetVoterTurnoutAsync(int electionId);
        Task<bool> HasUserVotedInElectionAsync(string userId, int electionId);
    }

    public class VoteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Vote Vote { get; set; }
        public string VerificationToken { get; set; }
    }
}

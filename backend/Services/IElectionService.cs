using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public interface IElectionService
    {
        Task<IEnumerable<Election>> GetAllElectionsAsync(int page, int pageSize);
        Task<IEnumerable<Election>> GetActiveElectionsAsync();
        Task<IEnumerable<Election>> GetUpcomingElectionsAsync();
        Task<IEnumerable<Election>> GetCompletedElectionsAsync();
        Task<Election> GetElectionByIdAsync(int id);
        Task<ServiceResult<Election>> CreateElectionAsync(CreateElectionModel model, string createdBy);
        Task<ServiceResult<Election>> UpdateElectionAsync(int id, UpdateElectionModel model, string updatedBy);
        Task<ServiceResult<bool>> DeleteElectionAsync(int id);
        Task<ServiceResult<bool>> StartElectionAsync(int id);
        Task<ServiceResult<bool>> EndElectionAsync(int id);
        Task<object> GetElectionResultsAsync(int id);
        Task<object> GetElectionStatisticsAsync(int id);

        Task<IEnumerable<Candidate>> GetAllCandidatesAsync(int page, int pageSize);
        Task<Candidate> GetCandidateByIdAsync(int id);
        Task<IEnumerable<Candidate>> GetCandidatesByElectionAsync(int electionId);
        Task<ServiceResult<Candidate>> AddCandidateAsync(int electionId, CreateCandidateModel model);
        Task<ServiceResult<Candidate>> UpdateCandidateAsync(int id, UpdateCandidateModel model);
        Task<ServiceResult<bool>> DeleteCandidateAsync(int id);
        Task<Candidate> GetCandidateProfileAsync(int id);
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public T Entity { get; set; }
    }
}

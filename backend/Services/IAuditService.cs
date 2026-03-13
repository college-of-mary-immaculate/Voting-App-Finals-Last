using Microsoft.AspNetCore.Http;

namespace VotingSystemBackend.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityType, string entityId, HttpRequest request);
    }
}

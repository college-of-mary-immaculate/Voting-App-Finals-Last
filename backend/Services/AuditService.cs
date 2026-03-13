using Microsoft.AspNetCore.Http;
using VotingSystemBackend.Data;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string action, string entityType, string entityId, HttpRequest request)
        {
            if (request == null)
                return;

            var ipAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = request.Headers["User-Agent"].ToString();
            var userId = request.HttpContext.User?.FindFirst("sub")?.Value;

            var log = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingSystemBackend.Models;
using VotingSystemBackend.Services;

namespace VotingSystemBackend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAuditService _auditService;
        private readonly IReportService _reportService;

        public AdminController(
            IAdminService adminService,
            IAuditService auditService,
            IReportService reportService)
        {
            _adminService = adminService;
            _auditService = auditService;
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardStatisticsAsync();
            return Ok(dashboard);
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var logs = await _adminService.GetAuditLogsAsync(from, to, page, pageSize);
            return Ok(logs);
        }

        [HttpGet("system-health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var health = await _adminService.GetSystemHealthAsync();
            return Ok(health);
        }

        [HttpPost("backup")]
        public async Task<IActionResult> CreateBackup()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var backup = await _adminService.CreateBackupAsync(userId);
            
            if (!backup.Success)
                return BadRequest(new { message = backup.Message });

            await _auditService.LogActionAsync("Backup Created", "System", backup.BackupId, Request);

            return Ok(new
            {
                message = "Backup created successfully",
                backupId = backup.BackupId,
                downloadUrl = backup.DownloadUrl
            });
        }

        [HttpGet("reports/voting-patterns")]
        public async Task<IActionResult> GetVotingPatternsReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var report = await _reportService.GenerateVotingPatternsReportAsync(startDate, endDate);
            return Ok(report);
        }

        [HttpGet("reports/demographics")]
        public async Task<IActionResult> GetDemographicReport([FromQuery] int? electionId)
        {
            var report = await _reportService.GenerateDemographicReportAsync(electionId);
            return Ok(report);
        }
    }
}
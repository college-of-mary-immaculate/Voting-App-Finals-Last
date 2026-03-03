// Controllers/ElectionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingSystemBackend.Models;
using VotingSystemBackend.Services;

namespace VotingSystemBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectionController : ControllerBase
    {
        private readonly IElectionService _electionService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public ElectionController(
            IElectionService electionService,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _electionService = electionService;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllElections([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var elections = await _electionService.GetAllElectionsAsync(page, pageSize);
            return Ok(elections);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveElections()
        {
            var elections = await _electionService.GetActiveElectionsAsync();
            return Ok(elections);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingElections()
        {
            var elections = await _electionService.GetUpcomingElectionsAsync();
            return Ok(elections);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedElections()
        {
            var elections = await _electionService.GetCompletedElectionsAsync();
            return Ok(elections);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetElection(int id)
        {
            var election = await _electionService.GetElectionByIdAsync(id);
            
            if (election == null)
                return NotFound();

            return Ok(election);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateElection([FromBody] CreateElectionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _electionService.CreateElectionAsync(model, userId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Election Created", "Election", result.Election.Id.ToString(), Request);

            return CreatedAtAction(nameof(GetElection), new { id = result.Election.Id }, result.Election);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateElection(int id, [FromBody] UpdateElectionModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _electionService.UpdateElectionAsync(id, model, userId);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Election Updated", "Election", id.ToString(), Request);

            return Ok(result.Election);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElection(int id)
        {
            var result = await _electionService.DeleteElectionAsync(id);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Election Deleted", "Election", id.ToString(), Request);

            return Ok(new { message = "Election deleted successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartElection(int id)
        {
            var result = await _electionService.StartElectionAsync(id);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _notificationService.SendElectionStartedNotificationAsync(id);
            await _auditService.LogActionAsync("Election Started", "Election", id.ToString(), Request);

            return Ok(new { message = "Election started successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/end")]
        public async Task<IActionResult> EndElection(int id)
        {
            var result = await _electionService.EndElectionAsync(id);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _notificationService.SendElectionEndedNotificationAsync(id);
            await _auditService.LogActionAsync("Election Ended", "Election", id.ToString(), Request);

            return Ok(new { message = "Election ended successfully" });
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetElectionResults(int id)
        {
            var results = await _electionService.GetElectionResultsAsync(id);
            
            if (results == null)
                return NotFound();

            return Ok(results);
        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetElectionStatistics(int id)
        {
            var statistics = await _electionService.GetElectionStatisticsAsync(id);
            
            if (statistics == null)
                return NotFound();

            return Ok(statistics);
        }
    }
}
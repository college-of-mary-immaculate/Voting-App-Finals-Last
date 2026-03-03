// Controllers/CandidateController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VotingSystemBackend.Models;
using VotingSystemBackend.Services;

namespace VotingSystemBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidateController : ControllerBase
    {
        private readonly IElectionService _electionService;
        private readonly IAuditService _auditService;

        public CandidateController(IElectionService electionService, IAuditService auditService)
        {
            _electionService = electionService;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCandidates([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var candidates = await _electionService.GetAllCandidatesAsync(page, pageSize);
            return Ok(candidates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCandidate(int id)
        {
            var candidate = await _electionService.GetCandidateByIdAsync(id);
            
            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }

        [HttpGet("election/{electionId}")]
        public async Task<IActionResult> GetCandidatesByElection(int electionId)
        {
            var candidates = await _electionService.GetCandidatesByElectionAsync(electionId);
            return Ok(candidates);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("election/{electionId}")]
        public async Task<IActionResult> AddCandidate(int electionId, [FromBody] CreateCandidateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _electionService.AddCandidateAsync(electionId, model);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Candidate Added", "Candidate", result.Candidate.Id.ToString(), Request);

            return CreatedAtAction(nameof(GetCandidate), new { id = result.Candidate.Id }, result.Candidate);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidate(int id, [FromBody] UpdateCandidateModel model)
        {
            var result = await _electionService.UpdateCandidateAsync(id, model);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Candidate Updated", "Candidate", id.ToString(), Request);

            return Ok(result.Candidate);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            var result = await _electionService.DeleteCandidateAsync(id);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _auditService.LogActionAsync("Candidate Deleted", "Candidate", id.ToString(), Request);

            return Ok(new { message = "Candidate deleted successfully" });
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetCandidateProfile(int id)
        {
            var profile = await _electionService.GetCandidateProfileAsync(id);
            
            if (profile == null)
                return NotFound();

            return Ok(profile);
        }
    }
}
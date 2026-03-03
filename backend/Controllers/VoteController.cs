// Controllers/VoteController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VotingSystemBackend.Models;
using VotingSystemBackend.Services;

namespace VotingSystemBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService _voteService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IEncryptionService _encryptionService;

        public VoteController(
            IVoteService voteService,
            IAuditService auditService,
            INotificationService notificationService,
            IEncryptionService encryptionService)
        {
            _voteService = voteService;
            _auditService = auditService;
            _notificationService = notificationService;
            _encryptionService = encryptionService;
        }

        [HttpPost]
        public async Task<IActionResult> CastVote([FromBody] CastVoteModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _voteService.CastVoteAsync(model, userId, ipAddress, userAgent);
            
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            await _notificationService.SendVoteConfirmationAsync(userId, result.Vote);
            await _auditService.LogActionAsync("Vote Cast", "Vote", result.Vote.Id.ToString(), Request);

            return Ok(new
            {
                message = "Vote cast successfully",
                voteId = result.Vote.Id,
                verificationToken = result.VerificationToken
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserVotes(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Users can only view their own votes unless they're admin
            if (currentUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var votes = await _voteService.GetUserVotesAsync(userId);
            return Ok(votes);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("election/{electionId}")]
        public async Task<IActionResult> GetElectionVotes(int electionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var votes = await _voteService.GetElectionVotesAsync(electionId, page, pageSize);
            return Ok(votes);
        }

        [HttpGet("verify/{voteId}")]
        public async Task<IActionResult> VerifyVote(int voteId)
        {
            var verification = await _voteService.VerifyVoteAsync(voteId);
            
            if (verification == null)
                return NotFound();

            return Ok(verification);
        }

        [HttpGet("turnout/{electionId}")]
        public async Task<IActionResult> GetVoterTurnout(int electionId)
        {
            var turnout = await _voteService.GetVoterTurnoutAsync(electionId);
            return Ok(turnout);
        }

        [HttpGet("check/{electionId}")]
        public async Task<IActionResult> CheckUserVoted(int electionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var hasVoted = await _voteService.HasUserVotedInElectionAsync(userId, electionId);
            
            return Ok(new { hasVoted });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VotingSystemBackend.Data;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public class VoteService : IVoteService
    {
        private readonly ApplicationDbContext _context;

        public VoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VoteResult> CastVoteAsync(CastVoteModel model, string userId, string ipAddress, string userAgent)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new VoteResult { Success = false, Message = "User must be authenticated to vote." };
            }

            var election = await _context.Elections.FindAsync(model.ElectionId);
            if (election == null)
            {
                return new VoteResult { Success = false, Message = "Election not found." };
            }

            if (election.Status != ElectionStatus.Active)
            {
                return new VoteResult { Success = false, Message = "Election is not active." };
            }

            var candidate = await _context.Candidates.FindAsync(model.CandidateId);
            if (candidate == null || candidate.ElectionId != model.ElectionId)
            {
                return new VoteResult { Success = false, Message = "Candidate not found for the specified election." };
            }

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ElectionId == model.ElectionId);

            if (existingVote != null)
            {
                return new VoteResult { Success = false, Message = "You have already voted in this election." };
            }

            var vote = new Vote
            {
                UserId = userId,
                ElectionId = model.ElectionId,
                CandidateId = model.CandidateId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                TransactionHash = Guid.NewGuid().ToString("N"),
                IsVerified = false,
                VerificationToken = Guid.NewGuid().ToString("N"),
                VotedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return new VoteResult
            {
                Success = true,
                Vote = vote,
                VerificationToken = vote.VerificationToken
            };
        }

        public async Task<IEnumerable<Vote>> GetUserVotesAsync(string userId)
        {
            return await _context.Votes
                .Where(v => v.UserId == userId)
                .Include(v => v.Candidate)
                .Include(v => v.Election)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vote>> GetElectionVotesAsync(int electionId, int page, int pageSize)
        {
            return await _context.Votes
                .Where(v => v.ElectionId == electionId)
                .Include(v => v.Candidate)
                .Include(v => v.User)
                .OrderByDescending(v => v.VotedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Vote> VerifyVoteAsync(int voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote == null) return null;

            vote.IsVerified = true;
            await _context.SaveChangesAsync();

            return vote;
        }

        public async Task<int> GetVoterTurnoutAsync(int electionId)
        {
            return await _context.Votes
                .Where(v => v.ElectionId == electionId)
                .Select(v => v.UserId)
                .Distinct()
                .CountAsync();
        }

        public async Task<bool> HasUserVotedInElectionAsync(string userId, int electionId)
        {
            return await _context.Votes
                .AnyAsync(v => v.UserId == userId && v.ElectionId == electionId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VotingSystemBackend.Data;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Services
{
    public class ElectionService : IElectionService
    {
        private readonly ApplicationDbContext _context;

        public ElectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Election>> GetAllElectionsAsync(int page, int pageSize)
        {
            return await _context.Elections
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Election>> GetActiveElectionsAsync()
        {
            return await _context.Elections
                .Where(e => e.Status == ElectionStatus.Active)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Election>> GetUpcomingElectionsAsync()
        {
            return await _context.Elections
                .Where(e => e.Status == ElectionStatus.Scheduled)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Election>> GetCompletedElectionsAsync()
        {
            return await _context.Elections
                .Where(e => e.Status == ElectionStatus.Completed)
                .OrderByDescending(e => e.EndDate)
                .ToListAsync();
        }

        public async Task<Election> GetElectionByIdAsync(int id)
        {
            return await _context.Elections
                .Include(e => e.Candidates)
                .Include(e => e.Votes)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<ServiceResult<Election>> CreateElectionAsync(CreateElectionModel model, string createdBy)
        {
            var election = new Election
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = ElectionStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                ElectionType = model.ElectionType,
                MaxVotesPerVoter = model.MaxVotesPerVoter,
                IsPublic = model.IsPublic,
                ElectionRules = model.ElectionRules
            };

            _context.Elections.Add(election);
            await _context.SaveChangesAsync();

            return new ServiceResult<Election>
            {
                Success = true,
                Entity = election
            };
        }

        public async Task<ServiceResult<Election>> UpdateElectionAsync(int id, UpdateElectionModel model, string updatedBy)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return new ServiceResult<Election>
                {
                    Success = false,
                    Message = "Election not found"
                };
            }

            if (!string.IsNullOrWhiteSpace(model.Title))
                election.Title = model.Title;
            if (!string.IsNullOrWhiteSpace(model.Description))
                election.Description = model.Description;
            if (model.StartDate.HasValue)
                election.StartDate = model.StartDate.Value;
            if (model.EndDate.HasValue)
                election.EndDate = model.EndDate.Value;
            if (!string.IsNullOrWhiteSpace(model.ElectionType))
                election.ElectionType = model.ElectionType;
            if (model.MaxVotesPerVoter.HasValue)
                election.MaxVotesPerVoter = model.MaxVotesPerVoter.Value;
            if (model.IsPublic.HasValue)
                election.IsPublic = model.IsPublic.Value;
            if (!string.IsNullOrWhiteSpace(model.ElectionRules))
                election.ElectionRules = model.ElectionRules;

            election.UpdatedAt = DateTime.UtcNow;

            _context.Elections.Update(election);
            await _context.SaveChangesAsync();

            return new ServiceResult<Election>
            {
                Success = true,
                Entity = election
            };
        }

        public async Task<ServiceResult<bool>> DeleteElectionAsync(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Election not found"
                };
            }

            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();

            return new ServiceResult<bool>
            {
                Success = true,
                Entity = true
            };
        }

        public async Task<ServiceResult<bool>> StartElectionAsync(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Election not found"
                };
            }

            election.Status = ElectionStatus.Active;
            election.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ServiceResult<bool> { Success = true, Entity = true };
        }

        public async Task<ServiceResult<bool>> EndElectionAsync(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Election not found"
                };
            }

            election.Status = ElectionStatus.Completed;
            election.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ServiceResult<bool> { Success = true, Entity = true };
        }

        public async Task<object> GetElectionResultsAsync(int id)
        {
            var election = await _context.Elections
                .Include(e => e.Candidates)
                    .ThenInclude(c => c.Votes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (election == null)
                return null;

            var results = election.Candidates
                .Select(c => new
                {
                    CandidateId = c.Id,
                    Name = $"{c.FirstName} {c.LastName}",
                    Votes = c.Votes?.Count ?? 0
                })
                .OrderByDescending(r => r.Votes)
                .ToList();

            return new
            {
                Election = new
                {
                    election.Id,
                    election.Title,
                    election.Status,
                    election.StartDate,
                    election.EndDate
                },
                Results = results
            };
        }

        public async Task<object> GetElectionStatisticsAsync(int id)
        {
            var election = await _context.Elections
                .Include(e => e.Votes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (election == null)
                return null;

            var totalVotes = election.Votes?.Count ?? 0;
            var uniqueVoters = election.Votes?.Select(v => v.UserId).Distinct().Count() ?? 0;

            return new
            {
                ElectionId = election.Id,
                TotalVotes = totalVotes,
                UniqueVoters = uniqueVoters,
                StartedAt = election.StartDate,
                EndedAt = election.EndDate
            };
        }

        public async Task<IEnumerable<Candidate>> GetAllCandidatesAsync(int page, int pageSize)
        {
            return await _context.Candidates
                .Include(c => c.Election)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Candidate> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates
                .Include(c => c.Election)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesByElectionAsync(int electionId)
        {
            return await _context.Candidates
                .Where(c => c.ElectionId == electionId)
                .Include(c => c.Election)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<ServiceResult<Candidate>> AddCandidateAsync(int electionId, CreateCandidateModel model)
        {
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null)
            {
                return new ServiceResult<Candidate>
                {
                    Success = false,
                    Message = "Election not found"
                };
            }

            var candidate = new Candidate
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Biography = model.Biography,
                PhotoUrl = model.PhotoUrl,
                Party = model.Party,
                Position = model.Position,
                Manifesto = model.Manifesto,
                CampaignSlogan = model.CampaignSlogan,
                Website = model.Website,
                SocialMediaLinks = model.SocialMediaLinks,
                ElectionId = electionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            return new ServiceResult<Candidate>
            {
                Success = true,
                Entity = candidate
            };
        }

        public async Task<ServiceResult<Candidate>> UpdateCandidateAsync(int id, UpdateCandidateModel model)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return new ServiceResult<Candidate>
                {
                    Success = false,
                    Message = "Candidate not found"
                };
            }

            if (!string.IsNullOrWhiteSpace(model.FirstName))
                candidate.FirstName = model.FirstName;
            if (!string.IsNullOrWhiteSpace(model.LastName))
                candidate.LastName = model.LastName;
            if (model.DateOfBirth.HasValue)
                candidate.DateOfBirth = model.DateOfBirth.Value;
            if (!string.IsNullOrWhiteSpace(model.Biography))
                candidate.Biography = model.Biography;
            if (!string.IsNullOrWhiteSpace(model.PhotoUrl))
                candidate.PhotoUrl = model.PhotoUrl;
            if (!string.IsNullOrWhiteSpace(model.Party))
                candidate.Party = model.Party;
            if (!string.IsNullOrWhiteSpace(model.Position))
                candidate.Position = model.Position;
            if (!string.IsNullOrWhiteSpace(model.Manifesto))
                candidate.Manifesto = model.Manifesto;
            if (!string.IsNullOrWhiteSpace(model.CampaignSlogan))
                candidate.CampaignSlogan = model.CampaignSlogan;
            if (!string.IsNullOrWhiteSpace(model.Website))
                candidate.Website = model.Website;
            if (!string.IsNullOrWhiteSpace(model.SocialMediaLinks))
                candidate.SocialMediaLinks = model.SocialMediaLinks;
            if (model.IsApproved.HasValue)
                candidate.IsApproved = model.IsApproved.Value;

            await _context.SaveChangesAsync();

            return new ServiceResult<Candidate>
            {
                Success = true,
                Entity = candidate
            };
        }

        public async Task<ServiceResult<bool>> DeleteCandidateAsync(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Candidate not found"
                };
            }

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return new ServiceResult<bool>
            {
                Success = true,
                Entity = true
            };
        }

        public async Task<Candidate> GetCandidateProfileAsync(int id)
        {
            return await _context.Candidates
                .Include(c => c.Election)
                .Include(c => c.Votes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

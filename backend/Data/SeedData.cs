// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VotingSystemBackend.Models;

namespace VotingSystemBackend.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            string[] roles = new[] { "Admin", "ElectionOfficer", "Voter" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user
            var adminEmail = "admin@votingsystem.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    VoterId = "ADMIN001",
                    IsEmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    await userManager.AddToRoleAsync(admin, "ElectionOfficer");
                }
            }

            // Seed sample election data if none exists
            if (!context.Elections.Any())
            {
                var election = new Election
                {
                    Title = "Presidential Election 2024",
                    Description = "National Presidential Election",
                    StartDate = DateTime.UtcNow.AddDays(30),
                    EndDate = DateTime.UtcNow.AddDays(37),
                    Status = ElectionStatus.Scheduled,
                    CreatedBy = "SYSTEM",
                    CreatedAt = DateTime.UtcNow,
                    ElectionType = "Presidential",
                    MaxVotesPerVoter = 1,
                    IsPublic = true
                };

                context.Elections.Add(election);
                await context.SaveChangesAsync();

                // Seed sample candidates
                var candidates = new[]
                {
                    new Candidate
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Biography = "Experienced leader",
                        Party = "Progressive Party",
                        Position = "President",
                        ElectionId = election.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Candidate
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Biography = "Community advocate",
                        Party = "Unity Party",
                        Position = "President",
                        ElectionId = election.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Candidates.AddRange(candidates);
                await context.SaveChangesAsync();
            }

            // Seed system settings
            if (!context.SystemSettings.Any())
            {
                var settings = new[]
                {
                    new SystemSetting { Key = "SiteName", Value = "Voting System", DataType = "string", Description = "Website name" },
                    new SystemSetting { Key = "MaxElectionsPerYear", Value = "12", DataType = "int", Description = "Maximum elections per year" },
                    new SystemSetting { Key = "VotingAgeLimit", Value = "18", DataType = "int", Description = "Minimum age to vote" },
                    new SystemSetting { Key = "EnableEmailNotifications", Value = "true", DataType = "boolean", Description = "Enable email notifications" },
                    new SystemSetting { Key = "MaintenanceMode", Value = "false", DataType = "boolean", Description = "System maintenance mode" }
                };

                context.SystemSettings.AddRange(settings);
                await context.SaveChangesAsync();
            }
        }
    }
}
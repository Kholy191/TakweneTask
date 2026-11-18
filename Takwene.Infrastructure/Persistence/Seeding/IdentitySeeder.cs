using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Takwene.Application.Common;
using Takwene.Infrastructure.Identity;

namespace Takwene.Infrastructure.Persistence.Seeding;

// Roles with their permission claims, then users from users.json. Safe to run on every startup.
internal sealed class IdentitySeeder(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedUsersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var (roleName, permissions) in Permissions.ByRole)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new IdentityRole(roleName);
                EnsureSucceeded(await roleManager.CreateAsync(role), $"create role {roleName}");
            }

            // Adds only missing claims, so new permissions reach existing databases.
            var existing = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var permission in permissions.Where(p => !existing.Contains(p)))
                EnsureSucceeded(await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission)), $"add {permission} to {roleName}");
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var seedUsers = await JsonDataSeeder.ReadAsync<SeedUser>("users.json", cancellationToken);

        foreach (var seedUser in seedUsers)
        {
            if (await userManager.FindByEmailAsync(seedUser.Email) is not null)
                continue;

            // Fixed ids so artists.json can link to these users.
            var user = new ApplicationUser
            {
                Id = seedUser.Id,
                UserName = seedUser.Email,
                Email = seedUser.Email,
                FullName = seedUser.FullName,
                EmailConfirmed = true
            };

            EnsureSucceeded(await userManager.CreateAsync(user, seedUser.Password), $"create user {seedUser.Email}");
            EnsureSucceeded(await userManager.AddToRoleAsync(user, seedUser.Role), $"add {seedUser.Email} to {seedUser.Role}");

            logger.LogInformation("Seeded user {Email} ({Role})", seedUser.Email, seedUser.Role);
        }
    }

    // Seed data is ours, so a failure here is a bug worth stopping startup for.
    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding failed to {action}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
    }
}

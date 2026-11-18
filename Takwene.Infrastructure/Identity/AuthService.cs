using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Takwene.Application.Common;
using Takwene.Application.Common.Errors;
using Takwene.Application.DTOs.Auth;
using Takwene.Application.Interfaces;
using Takwene.Domain.Entities;
using Takwene.Infrastructure.Persistence;

namespace Takwene.Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    TokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result<string>> CreateUserAsync(string fullName, string email, string password, string role, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return AuthErrors.EmailAlreadyExists;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return AuthErrors.UserCreationFailed(Describe(created));

        var roleAdded = await userManager.AddToRoleAsync(user, role);
        if (!roleAdded.Succeeded)
            return AuthErrors.UserCreationFailed(Describe(roleAdded));

        logger.LogInformation("User {UserId} created in role {Role}", user.Id, role);
        return user.Id;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Same error for unknown email and wrong password, so accounts can't be probed.
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return AuthErrors.InvalidCredentials;

        return await IssueTokensAsync(user, replacedToken: null, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var storedToken = await FindTokenAsync(request.RefreshToken, includeUser: true, cancellationToken);
        if (storedToken is null)
            return AuthErrors.InvalidRefreshToken;

        // A revoked token being reused means it was likely stolen: revoke every token for this user.
        if (storedToken.RevokedOn is not null)
        {
            await RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            logger.LogWarning("Revoked refresh token reused for user {UserId}; all sessions revoked", storedToken.UserId);
            return AuthErrors.InvalidRefreshToken;
        }

        if (!storedToken.IsActive)
            return AuthErrors.InvalidRefreshToken;

        return await IssueTokensAsync(storedToken.User, storedToken, cancellationToken);
    }

    public async Task<Result> RevokeRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var storedToken = await FindTokenAsync(request.RefreshToken, includeUser: false, cancellationToken);
        if (storedToken is null || !storedToken.IsActive)
            return AuthErrors.InvalidRefreshToken;

        storedToken.RevokedOn = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // Creates a new token pair and, on rotation, revokes the token it replaces in the same save.
    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, RefreshToken? replacedToken, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(user.Id, cancellationToken);

        // Linked artist profile, if any, so the API can scope data without another lookup.
        var artistId = await context.Set<Artist>()
            .Where(a => a.UserId == user.Id)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var (accessToken, accessTokenExpiresAt) = tokenService.CreateAccessToken(user, roles, permissions, artistId);

        var rawRefreshToken = TokenService.CreateRefreshToken();
        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            TokenHash = TokenService.Hash(rawRefreshToken),
            UserId = user.Id,
            CreatedOn = now,
            ExpiresOn = now.AddDays(_jwt.RefreshTokenDays)
        };
        context.RefreshTokens.Add(refreshToken);

        if (replacedToken is not null)
        {
            replacedToken.RevokedOn = now;
            replacedToken.ReplacedByTokenHash = refreshToken.TokenHash;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email ?? string.Empty,
            accessToken,
            accessTokenExpiresAt,
            rawRefreshToken,
            refreshToken.ExpiresOn);
    }

    // Permission claims of all the user's roles, in one query.
    private Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken) =>
        context.RoleClaims
            .Where(rc => rc.ClaimType == Permissions.ClaimType
                         && context.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rc.RoleId))
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync(cancellationToken);

    private static string Describe(IdentityResult result) => string.Join(" ", result.Errors.Select(e => e.Description));

    private Task<RefreshToken?> FindTokenAsync(string rawToken, bool includeUser, CancellationToken cancellationToken)
    {
        var hash = TokenService.Hash(rawToken);
        var query = context.RefreshTokens.AsQueryable();

        if (includeUser)
            query = query.Include(t => t.User);

        return query.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
    }

    private async Task RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedOn == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in activeTokens)
            token.RevokedOn = now;

        await context.SaveChangesAsync(cancellationToken);
    }
}

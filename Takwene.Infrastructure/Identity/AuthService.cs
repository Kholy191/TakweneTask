using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Takwene.Application.Auth;
using Takwene.Application.Common;
using Takwene.Application.Interfaces;
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

    public async Task<Result<string>> RegisterUserAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return AuthErrors.EmailAlreadyExists;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return AuthErrors.RegistrationFailed(string.Join(" ", result.Errors.Select(e => e.Description)));

        logger.LogInformation("User {UserId} registered", user.Id);
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
        var (accessToken, accessTokenExpiresAt) = tokenService.CreateAccessToken(user, roles);

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

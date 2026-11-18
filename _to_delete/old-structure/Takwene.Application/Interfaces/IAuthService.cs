using Takwene.Application.Auth;
using Takwene.Application.Common;

namespace Takwene.Application.Interfaces;

// The only gateway from Application to Identity; implemented in Infrastructure.
public interface IAuthService
{
    // Creates an Identity user in the given role; returns the new user's id.
    Task<Result<string>> CreateUserAsync(string fullName, string email, string password, string role, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    // Rotates the refresh token: the old one is revoked and a new pair is issued.
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}

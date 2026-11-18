namespace Takwene.Application.Auth;

public sealed record AuthResponse(
    string UserId,
    string Email,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

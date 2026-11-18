using Takwene.Application.Common;

namespace Takwene.Application.Auth;

public static class AuthErrors
{
    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Auth.EmailAlreadyExists", "An account with this email already exists.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid, expired or revoked.");

    public static Error RegistrationFailed(string details) =>
        Error.Validation("Auth.RegistrationFailed", details);
}

using System.ComponentModel.DataAnnotations;

namespace Takwene.Application.Auth;

public sealed record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

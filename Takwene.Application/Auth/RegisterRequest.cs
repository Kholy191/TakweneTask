using System.ComponentModel.DataAnnotations;

namespace Takwene.Application.Auth;

public sealed record RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    // Identity also requires a digit, upper case, lower case and a symbol.
    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;
}

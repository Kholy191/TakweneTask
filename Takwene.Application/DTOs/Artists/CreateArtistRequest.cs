namespace Takwene.Application.DTOs.Artists;

public sealed record CreateArtistRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    // Optional; a temporary password is generated and returned once when omitted.
    public string? Password { get; init; }
}

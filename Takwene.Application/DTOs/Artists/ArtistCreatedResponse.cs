namespace Takwene.Application.DTOs.Artists;

// TemporaryPassword is only set when the admin did not provide one.
public sealed record ArtistCreatedResponse(Guid Id, string Name, string Email, string Country, string UserId, string? TemporaryPassword);

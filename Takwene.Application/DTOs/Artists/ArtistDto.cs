namespace Takwene.Application.DTOs.Artists;

public sealed record ArtistDto(Guid Id, string Name, string Email, string Country, string? UserId, int TrackCount);

using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

public sealed record TrackDetailsDto(
    Guid Id,
    string Title,
    string Isrc,
    string Genre,
    TrackStatus Status,
    DateOnly ReleaseDate,
    DateTime CreatedAt,
    TrackArtistDto Artist,
    List<TrackDistributionDto> Distributions);

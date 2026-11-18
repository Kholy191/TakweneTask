using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

public sealed record TrackListItemDto(
    Guid Id,
    string Title,
    string Isrc,
    string Genre,
    TrackStatus Status,
    DateOnly ReleaseDate,
    Guid ArtistId,
    string ArtistName);

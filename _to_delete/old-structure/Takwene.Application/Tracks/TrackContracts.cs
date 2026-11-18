using Takwene.Application.Common;
using Takwene.Domain.Enums;

namespace Takwene.Application.Tracks;

public sealed record CreateTrackRequest
{
    public string Title { get; init; } = string.Empty;
    public string Isrc { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public DateOnly ReleaseDate { get; init; }

    // Required for admins; ignored for artists (they always create for themselves).
    public Guid? ArtistId { get; init; }
}

public sealed class TrackQueryParameters : PagedQuery
{
    public Guid? ArtistId { get; set; }
    public string? Genre { get; set; }
    public TrackStatus? Status { get; set; }
    public string? Search { get; set; }
}

public sealed record DistributeTrackRequest
{
    public List<Guid> DspIds { get; init; } = [];
}

public sealed record UpdateTrackStatusRequest
{
    public TrackStatus Status { get; init; }
}

public sealed record UpdateDistributionStatusRequest
{
    public DistributionStatus Status { get; init; }
}

public sealed record TrackListItemDto(
    Guid Id,
    string Title,
    string Isrc,
    string Genre,
    TrackStatus Status,
    DateOnly ReleaseDate,
    Guid ArtistId,
    string ArtistName);

public sealed record TrackArtistDto(Guid Id, string Name, string Country);

public sealed record TrackDistributionDto(Guid Id, Guid DspId, string DspName, DistributionStatus Status, DateTime SubmittedAt);

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

// What happened to each requested DSP.
public sealed record DistributeTrackResponse(
    Guid TrackId,
    TrackStatus TrackStatus,
    IReadOnlyList<Guid> Submitted,
    IReadOnlyList<Guid> Resubmitted,
    IReadOnlyList<Guid> Skipped);

public static class TrackErrors
{
    public static readonly Error NotFound = Error.NotFound("Tracks.NotFound", "Track not found.");

    public static readonly Error DistributionNotFound =
        Error.NotFound("Tracks.DistributionNotFound", "Distribution not found for this track.");

    public static readonly Error ArtistIdRequired =
        Error.Validation("Tracks.ArtistIdRequired", "ArtistId is required when an admin creates a track.");

    public static readonly Error IsrcAlreadyExists =
        Error.Conflict("Tracks.IsrcAlreadyExists", "A track with this ISRC already exists.");

    public static readonly Error StatusUnchanged =
        Error.Validation("Tracks.StatusUnchanged", "The track already has this status.");

    public static Error InvalidStatusTransition(TrackStatus from, TrackStatus to) =>
        Error.Conflict("Tracks.InvalidStatusTransition", $"Cannot change track status from {from} to {to}.");

    public static Error InvalidDistributionTransition(DistributionStatus from, DistributionStatus to) =>
        Error.Conflict("Tracks.InvalidDistributionTransition", $"Cannot change distribution status from {from} to {to}.");

    public static Error UnknownDsps(IEnumerable<Guid> dspIds) =>
        Error.Validation("Tracks.UnknownDsps", $"Unknown DSP ids: {string.Join(", ", dspIds)}.");
}

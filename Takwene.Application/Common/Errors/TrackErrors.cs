using Takwene.Domain.Enums;

namespace Takwene.Application.Common.Errors;

public static class TrackErrors
{
    public static readonly Error NotFound = Error.NotFound("Tracks.NotFound", "Track not found.");

    public static readonly Error ArtistIdRequired =
        Error.Validation("Tracks.ArtistIdRequired", "ArtistId is required when an admin creates a track.");

    public static readonly Error IsrcAlreadyExists =
        Error.Conflict("Tracks.IsrcAlreadyExists", "A track with this ISRC already exists.");

    public static readonly Error StatusUnchanged =
        Error.Validation("Tracks.StatusUnchanged", "The track already has this status.");

    public static Error InvalidStatusTransition(TrackStatus from, TrackStatus to) =>
        Error.Conflict("Tracks.InvalidStatusTransition", $"Cannot change track status from {from} to {to}.");
}

using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

// What happened to each requested DSP.
public sealed record DistributeTrackResponse(
    Guid TrackId,
    TrackStatus TrackStatus,
    IReadOnlyList<Guid> Submitted,
    IReadOnlyList<Guid> Resubmitted,
    IReadOnlyList<Guid> Skipped);

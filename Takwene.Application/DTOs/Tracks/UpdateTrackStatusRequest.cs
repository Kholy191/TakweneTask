using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

public sealed record UpdateTrackStatusRequest
{
    public TrackStatus Status { get; init; }
}

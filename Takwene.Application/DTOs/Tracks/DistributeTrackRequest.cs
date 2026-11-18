namespace Takwene.Application.DTOs.Tracks;

public sealed record DistributeTrackRequest
{
    public List<Guid> DspIds { get; init; } = [];
}

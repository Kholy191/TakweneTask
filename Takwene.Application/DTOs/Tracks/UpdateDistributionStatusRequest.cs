using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

public sealed record UpdateDistributionStatusRequest
{
    public DistributionStatus Status { get; init; }
}

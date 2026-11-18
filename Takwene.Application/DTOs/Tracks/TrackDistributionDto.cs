using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

public sealed record TrackDistributionDto(Guid Id, Guid DspId, string DspName, DistributionStatus Status, DateTime SubmittedAt);

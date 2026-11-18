using Takwene.Domain.Common;
using Takwene.Domain.Enums;

namespace Takwene.Domain.Entities;

// A track's delivery to one DSP; unique per (TrackId, DspId).
public class TrackDistribution : BaseEntity
{
    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;

    public Guid DspId { get; set; }
    public Dsp Dsp { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }
    public DistributionStatus Status { get; set; } = DistributionStatus.Pending;
}

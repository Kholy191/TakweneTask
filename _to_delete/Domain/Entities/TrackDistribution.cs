using Takwene.Domain.Common;
using Takwene.Domain.Enums;

namespace Takwene.Domain.Entities;

// Links a Track to a DSP and tracks its delivery status there.
public class TrackDistribution : BaseEntity
{
    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;

    public Guid DspId { get; set; }
    public Dsp Dsp { get; set; } = null!;

    public DistributionStatus Status { get; set; } = DistributionStatus.Pending;
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? LiveAtUtc { get; set; }

    // Public link to the track on the DSP once live.
    public string? ExternalUrl { get; set; }
}

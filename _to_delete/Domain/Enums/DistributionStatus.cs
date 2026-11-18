namespace Takwene.Domain.Enums;

// Lifecycle of a track on a single DSP.
public enum DistributionStatus
{
    Pending = 0,
    Submitted = 1,
    Live = 2,
    Rejected = 3,
    TakenDown = 4
}

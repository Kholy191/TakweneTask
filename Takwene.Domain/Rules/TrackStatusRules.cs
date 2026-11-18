using Takwene.Domain.Enums;

namespace Takwene.Domain.Rules;

public static class TrackStatusRules
{
    // Manual changes only; Draft -> Submitted happens through distribution, Distributed is final.
    public static bool CanChange(TrackStatus from, TrackStatus to, bool hasLiveDistribution) => (from, to) switch
    {
        (TrackStatus.Submitted, TrackStatus.Distributed) => hasLiveDistribution,
        (TrackStatus.Submitted, TrackStatus.Draft) => !hasLiveDistribution,
        _ => false
    };
}

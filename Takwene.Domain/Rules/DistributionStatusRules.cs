using Takwene.Domain.Enums;

namespace Takwene.Domain.Rules;

public static class DistributionStatusRules
{
    // A DSP decides once; a rejected distribution goes back to Pending only by distributing again.
    public static bool CanChange(DistributionStatus from, DistributionStatus to) =>
        from == DistributionStatus.Pending && to is DistributionStatus.Live or DistributionStatus.Rejected;
}

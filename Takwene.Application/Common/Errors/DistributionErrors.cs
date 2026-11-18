using Takwene.Domain.Enums;

namespace Takwene.Application.Common.Errors;

public static class DistributionErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Distributions.NotFound", "Distribution not found for this track.");

    public static Error UnknownDsps(IEnumerable<Guid> dspIds) =>
        Error.Validation("Distributions.UnknownDsps", $"Unknown DSP ids: {string.Join(", ", dspIds)}.");

    public static Error InvalidStatusTransition(DistributionStatus from, DistributionStatus to) =>
        Error.Conflict("Distributions.InvalidStatusTransition", $"Cannot change distribution status from {from} to {to}.");
}

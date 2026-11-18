using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Dsps;

// Returns which of the given DSP ids exist.
public sealed class DspIdsSpecification : BaseSpecification<Dsp, Guid>
{
    public DspIdsSpecification(IReadOnlyCollection<Guid> dspIds) : base(d => dspIds.Contains(d.Id))
    {
        ApplySelector(d => d.Id);
    }
}

using Takwene.Application.DTOs.Dsps;
using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Dsps;

public sealed class DspListSpecification : BaseSpecification<Dsp, DspDto>
{
    public DspListSpecification()
    {
        ApplySelector(d => new DspDto(d.Id, d.Name));
        ApplyOrderBy(d => d.Name);
    }
}

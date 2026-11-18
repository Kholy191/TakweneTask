using Takwene.Application.Interfaces;
using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Dsps;

public sealed record DspDto(Guid Id, string Name);

public sealed class DspListSpecification : BaseSpecification<Dsp, DspDto>
{
    public DspListSpecification()
    {
        ApplySelector(d => new DspDto(d.Id, d.Name));
        ApplyOrderBy(d => d.Name);
    }
}

public sealed class DspIdsSpecification : BaseSpecification<Dsp, Guid>
{
    public DspIdsSpecification(IReadOnlyCollection<Guid> dspIds) : base(d => dspIds.Contains(d.Id))
    {
        ApplySelector(d => d.Id);
    }
}

public interface IDspService
{
    Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

internal sealed class DspService(IUnitOfWork unitOfWork) : IDspService
{
    public Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        unitOfWork.Repository<Dsp>().ListAsync(new DspListSpecification(), cancellationToken);
}

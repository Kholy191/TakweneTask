using Takwene.Application.DTOs.Dsps;
using Takwene.Application.Interfaces;
using Takwene.Application.Specifications.Dsps;
using Takwene.Domain.Entities;

namespace Takwene.Application.Services;

internal sealed class DspService(IUnitOfWork unitOfWork) : IDspService
{
    public Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        unitOfWork.Repository<Dsp>().ListAsync(new DspListSpecification(), cancellationToken);
}

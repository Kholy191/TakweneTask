using Takwene.Application.DTOs.Dsps;

namespace Takwene.Application.Interfaces;

public interface IDspService
{
    Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

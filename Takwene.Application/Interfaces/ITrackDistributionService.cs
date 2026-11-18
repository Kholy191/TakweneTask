using Takwene.Application.Common;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Interfaces;

// Sending tracks to DSPs and recording the DSPs' decisions.
public interface ITrackDistributionService
{
    Task<Result<DistributeTrackResponse>> DistributeAsync(Guid trackId, DistributeTrackRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateDistributionStatusAsync(Guid trackId, Guid distributionId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken = default);
}

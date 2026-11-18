using Takwene.Application.Common;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Interfaces;

// Track catalogue and lifecycle. Every method is scoped to the caller's own tracks unless they are admin.
public interface ITrackService
{
    Task<Result<TrackDetailsDto>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<TrackListItemDto>>> GetAllAsync(TrackQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken = default);
}

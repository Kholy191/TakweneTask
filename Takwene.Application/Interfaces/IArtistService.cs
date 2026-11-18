using Takwene.Application.Common;
using Takwene.Application.DTOs.Artists;

namespace Takwene.Application.Interfaces;

public interface IArtistService
{
    // Creates the artist and its login in one transaction.
    Task<Result<ArtistCreatedResponse>> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<ArtistDto>> GetAllAsync(PagedQuery query, CancellationToken cancellationToken = default);
}

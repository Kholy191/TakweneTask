using Takwene.Application.DTOs.Artists;
using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Artists;

public sealed class ArtistListSpecification : BaseSpecification<Artist, ArtistDto>
{
    public ArtistListSpecification(int page, int pageSize)
    {
        ApplySelector(a => new ArtistDto(a.Id, a.Name, a.Email, a.Country, a.UserId, a.Tracks.Count));
        ApplyOrderBy(a => a.Name);
        ApplyPaging(page, pageSize);
    }
}

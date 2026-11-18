using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Tracks;

// Tracked track with its distributions, for write operations; scoped like TrackDetailsSpecification.
public sealed class TrackForUpdateSpecification : BaseSpecification<Track>
{
    public TrackForUpdateSpecification(Guid trackId, Guid? artistScope)
        : base(t => t.Id == trackId && (artistScope == null || t.ArtistId == artistScope))
    {
        AddInclude(t => t.Distributions);
    }
}

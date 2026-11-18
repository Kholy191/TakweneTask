using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Tracks;

// A distribution of the given track, scoped to the caller's own tracks unless admin.
public sealed class DistributionForUpdateSpecification(Guid trackId, Guid distributionId, Guid? artistScope)
    : BaseSpecification<TrackDistribution>(d =>
        d.Id == distributionId
        && d.TrackId == trackId
        && (artistScope == null || d.Track.ArtistId == artistScope));

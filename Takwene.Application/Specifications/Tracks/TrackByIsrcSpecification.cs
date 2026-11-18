using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Tracks;

public sealed class TrackByIsrcSpecification(string normalizedIsrc)
    : BaseSpecification<Track>(t => t.Isrc == normalizedIsrc);

using Takwene.Application.DTOs.Tracks;
using Takwene.Domain.Entities;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Tracks;

// artistScope == null means admin; otherwise another artist's track is simply not found.
public sealed class TrackDetailsSpecification : BaseSpecification<Track, TrackDetailsDto>
{
    public TrackDetailsSpecification(Guid trackId, Guid? artistScope)
        : base(t => t.Id == trackId && (artistScope == null || t.ArtistId == artistScope))
    {
        ApplySelector(t => new TrackDetailsDto(
            t.Id,
            t.Title,
            t.Isrc,
            t.Genre,
            t.Status,
            t.ReleaseDate,
            t.CreatedAt,
            new TrackArtistDto(t.Artist.Id, t.Artist.Name, t.Artist.Country),
            t.Distributions
                .OrderBy(d => d.SubmittedAt)
                .Select(d => new TrackDistributionDto(d.Id, d.DspId, d.Dsp.Name, d.Status, d.SubmittedAt))
                .ToList()));
    }
}

using Takwene.Domain.Entities;
using Takwene.Domain.Enums;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Tracks.Specifications;

// Ownership is part of every query: artistScope == null means admin (no restriction).

public sealed class TrackListSpecification : BaseSpecification<Track, TrackListItemDto>
{
    public TrackListSpecification(Guid? artistId, string? genre, TrackStatus? status, string? search, int page, int pageSize)
        : base(BuildCriteria(artistId, genre, status))
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            AddSearch(t => t.Title, term);
            AddSearch(t => t.Artist.Name, term);

            // ISRCs are stored without hyphens, so search them the same way.
            AddSearch(t => t.Isrc, term.Replace("-", string.Empty));
        }

        ApplySelector(t => new TrackListItemDto(
            t.Id, t.Title, t.Isrc, t.Genre, t.Status, t.ReleaseDate, t.ArtistId, t.Artist.Name));

        // Guid v7 ids are time-ordered and unique: newest first, stable across pages.
        ApplyOrderByDescending(t => t.Id);
        ApplyPaging(page, pageSize);
    }

    private static System.Linq.Expressions.Expression<Func<Track, bool>> BuildCriteria(Guid? artistId, string? genre, TrackStatus? status)
    {
        // Genre is an exact, case-insensitive filter (lower() = lower()).
        var normalizedGenre = string.IsNullOrWhiteSpace(genre) ? null : genre.Trim().ToLowerInvariant();

        return t => (artistId == null || t.ArtistId == artistId)
                    && (normalizedGenre == null || t.Genre.ToLower() == normalizedGenre)
                    && (status == null || t.Status == status);
    }
}

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

// Tracked entity with its distributions, for write operations.
public sealed class TrackForUpdateSpecification : BaseSpecification<Track>
{
    public TrackForUpdateSpecification(Guid trackId, Guid? artistScope)
        : base(t => t.Id == trackId && (artistScope == null || t.ArtistId == artistScope))
    {
        AddInclude(t => t.Distributions);
    }
}

public sealed class TrackByIsrcSpecification(string normalizedIsrc)
    : BaseSpecification<Track>(t => t.Isrc == normalizedIsrc);

public sealed class DistributionForUpdateSpecification(Guid trackId, Guid distributionId, Guid? artistScope)
    : BaseSpecification<TrackDistribution>(d =>
        d.Id == distributionId
        && d.TrackId == trackId
        && (artistScope == null || d.Track.ArtistId == artistScope));

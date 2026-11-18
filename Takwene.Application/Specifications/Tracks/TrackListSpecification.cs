using System.Linq.Expressions;
using Takwene.Application.DTOs.Tracks;
using Takwene.Domain.Entities;
using Takwene.Domain.Enums;
using Takwene.Domain.Specifications;

namespace Takwene.Application.Specifications.Tracks;

// artistId == null means no artist filter (admins only; services always pin artists to their own id).
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

    private static Expression<Func<Track, bool>> BuildCriteria(Guid? artistId, string? genre, TrackStatus? status)
    {
        // Genre is an exact, case-insensitive filter (lower() = lower()).
        var normalizedGenre = string.IsNullOrWhiteSpace(genre) ? null : genre.Trim().ToLowerInvariant();

        return t => (artistId == null || t.ArtistId == artistId)
                    && (normalizedGenre == null || t.Genre.ToLower() == normalizedGenre)
                    && (status == null || t.Status == status);
    }
}

using Takwene.Domain.Common;
using Takwene.Domain.Enums;

namespace Takwene.Domain.Entities;

public class Track : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    // Stored normalized: upper case, no hyphens (see Isrc.Normalize).
    public string Isrc { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public TrackStatus Status { get; set; } = TrackStatus.Draft;

    public Guid ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public ICollection<TrackDistribution> Distributions { get; set; } = [];
}

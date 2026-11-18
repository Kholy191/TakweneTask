using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

public class Track : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    // International Standard Recording Code (unique per recording).
    public string? Isrc { get; set; }

    public string? Genre { get; set; }
    public TimeSpan Duration { get; set; }
    public DateOnly? ReleaseDate { get; set; }

    public Guid ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}

using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

// Digital Service Provider (e.g. Spotify, Anghami, Apple Music).
public class Dsp : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TrackDistribution> Distributions { get; set; } = new List<TrackDistribution>();
}

using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Country { get; set; }

    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}

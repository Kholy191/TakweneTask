using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Identity user id (ApplicationUser lives in Infrastructure).
    public string? UserId { get; set; }

    public ICollection<Track> Tracks { get; set; } = [];
}

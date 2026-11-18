using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

// Digital Service Provider, e.g. Spotify.
public class Dsp : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

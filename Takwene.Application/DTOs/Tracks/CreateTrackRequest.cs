namespace Takwene.Application.DTOs.Tracks;

public sealed record CreateTrackRequest
{
    public string Title { get; init; } = string.Empty;
    public string Isrc { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public DateOnly ReleaseDate { get; init; }

    // Required for admins; ignored for artists (they always create for themselves).
    public Guid? ArtistId { get; init; }
}

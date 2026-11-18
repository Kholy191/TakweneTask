namespace Takwene.Application.Interfaces;

// The authenticated caller, read from JWT claims. Empty for anonymous requests and background work.
public interface ICurrentUserService
{
    string? UserId { get; }

    // From the "artist_id" claim; null when the user has no linked artist.
    Guid? ArtistId { get; }

    bool IsAdmin { get; }

    bool HasPermission(string permission);
}

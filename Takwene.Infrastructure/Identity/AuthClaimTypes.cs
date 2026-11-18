namespace Takwene.Infrastructure.Identity;

// Short claim names, kept as-is because inbound claim mapping is turned off.
internal static class AuthClaimTypes
{
    public const string Name = "name";
    public const string Role = "role";
    public const string ArtistId = "artist_id";
}

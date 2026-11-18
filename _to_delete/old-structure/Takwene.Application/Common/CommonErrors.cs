namespace Takwene.Application.Common;

public static class CommonErrors
{
    // Non-admin caller whose account has no linked artist: deny instead of falling back to "all data".
    public static readonly Error ArtistProfileMissing =
        Error.Forbidden("Auth.ArtistProfileMissing", "Your account is not linked to an artist profile.");
}

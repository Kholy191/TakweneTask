namespace Takwene.Application.Common.Errors;

public static class CommonErrors
{
    // Non-admin caller whose account has no linked artist.
    public static readonly Error ArtistProfileMissing =
        Error.Forbidden("Auth.ArtistProfileMissing", "Your account is not linked to an artist profile.");
}

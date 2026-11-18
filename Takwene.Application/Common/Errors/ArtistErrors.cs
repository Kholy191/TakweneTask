namespace Takwene.Application.Common.Errors;

public static class ArtistErrors
{
    public static readonly Error NotFound = Error.NotFound("Artists.NotFound", "Artist not found.");

    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Artists.EmailAlreadyExists", "An artist with this email already exists.");
}

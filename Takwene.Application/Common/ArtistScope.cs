using Takwene.Application.Common.Errors;
using Takwene.Application.Interfaces;

namespace Takwene.Application.Common;

// Decides whose data the caller may touch. Shared by every service that reads or writes artist-owned data.
public static class ArtistScope
{
    // Value null = admin (no restriction). Non-admins without a linked artist are denied, never unscoped.
    public static Result<Guid?> For(ICurrentUserService currentUser)
    {
        if (currentUser.IsAdmin)
            return Result.Success<Guid?>(null);

        return currentUser.ArtistId is { } artistId
            ? Result.Success<Guid?>(artistId)
            : Result.Failure<Guid?>(CommonErrors.ArtistProfileMissing);
    }
}

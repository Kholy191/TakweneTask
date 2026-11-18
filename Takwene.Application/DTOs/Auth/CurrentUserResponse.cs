namespace Takwene.Application.DTOs.Auth;

// What the caller's access token carries.
public sealed record CurrentUserResponse(
    string? UserId,
    Guid? ArtistId,
    bool IsAdmin,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

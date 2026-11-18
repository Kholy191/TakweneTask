using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Takwene.Application.Common;
using Takwene.Application.Interfaces;

namespace Takwene.Infrastructure.Identity;

// Reads the caller from JWT claims. Empty outside an HTTP request (e.g. startup seeding).
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    public Guid? ArtistId => Guid.TryParse(User?.FindFirst(AuthClaimTypes.ArtistId)?.Value, out var artistId) ? artistId : null;

    public bool IsAdmin => User?.HasClaim(AuthClaimTypes.Role, Roles.Admin) ?? false;

    public bool HasPermission(string permission) => User?.HasClaim(Permissions.ClaimType, permission) ?? false;

    public IReadOnlyList<string> GetRoles() => GetClaimValues(AuthClaimTypes.Role);

    public IReadOnlyList<string> GetPermissions() => GetClaimValues(Permissions.ClaimType);

    private IReadOnlyList<string> GetClaimValues(string claimType) =>
        User?.FindAll(claimType).Select(c => c.Value).ToList() ?? [];
}

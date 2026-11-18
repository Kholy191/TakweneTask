using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takwene.Application.DTOs.Auth;
using Takwene.Application.Interfaces;

namespace Takwene.API.Controllers;

// Accounts are created by admins (POST /api/artists); there is no public sign-up.
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpPost("login")]
    [EndpointName("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPost("refresh")]
    [EndpointName("RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    // No access token required, so a user whose token expired can still log out; the refresh token is the proof.
    [HttpPost("logout")]
    [EndpointName("Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RevokeRefreshTokenAsync(request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [Authorize]
    [HttpGet("me")]
    [EndpointName("GetCurrentUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CurrentUserResponse> Me() => Ok(new CurrentUserResponse(
        currentUser.UserId,
        currentUser.ArtistId,
        currentUser.IsAdmin,
        currentUser.GetRoles(),
        currentUser.GetPermissions()));
}

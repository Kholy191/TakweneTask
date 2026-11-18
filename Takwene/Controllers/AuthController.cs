using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takwene.Application.Auth;
using Takwene.Application.Interfaces;

namespace Takwene.API.Controllers;

[Route("api/auth")]
public sealed class AuthController(IAuthService authService, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterUserAsync(request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new { userId = result.Value })
            : HandleFailure(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    // Logout: revokes the given refresh token.
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RevokeRefreshTokenAsync(request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    // Quick check that the access token works.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        userId = currentUser.UserId,
        name = User.Identity?.Name,
        roles = User.FindAll("role").Select(c => c.Value)
    });
}

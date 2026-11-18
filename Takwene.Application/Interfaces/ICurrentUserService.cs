namespace Takwene.Application.Interfaces;

// The authenticated caller, read from the JWT "sub" claim. Null for anonymous requests and background work.
public interface ICurrentUserService
{
    string? UserId { get; }
}

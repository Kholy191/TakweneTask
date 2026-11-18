using Microsoft.AspNetCore.Identity;

namespace Takwene.Infrastructure.Identity;

internal sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

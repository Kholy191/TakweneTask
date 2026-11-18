using Takwene.Domain.Common;

namespace Takwene.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    // Identity user id; kept as a plain string so Domain stays free of Identity packages.
    public string UserId { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    // Set when this token is rotated, to trace the replacement.
    public string? ReplacedByToken { get; set; }

    // Computed (not mapped by EF Core).
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
}

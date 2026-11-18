namespace Takwene.Infrastructure.Identity;

internal sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    // SHA-256 hash of the token; the raw value is only ever sent to the client.
    public string TokenHash { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    // Set when the token is rotated, to trace the replacement.
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedOn is null && DateTime.UtcNow < ExpiresOn;
}

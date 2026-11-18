using Takwene.Domain.Interfaces;

namespace Takwene.Domain.Common;

public abstract class BaseEntity : IAuditableEntity
{
    // Version 7 GUIDs are time-ordered, which keeps PostgreSQL indexes efficient.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

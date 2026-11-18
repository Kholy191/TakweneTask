namespace Takwene.Domain.Interfaces;

// Audit fields filled automatically by the EF Core interceptor.
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

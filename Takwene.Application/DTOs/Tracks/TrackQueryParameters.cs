using Takwene.Application.Common;
using Takwene.Domain.Enums;

namespace Takwene.Application.DTOs.Tracks;

// Query string of GET /api/tracks.
public sealed class TrackQueryParameters : PagedQuery
{
    public Guid? ArtistId { get; set; }
    public string? Genre { get; set; }
    public TrackStatus? Status { get; set; }
    public string? Search { get; set; }
}

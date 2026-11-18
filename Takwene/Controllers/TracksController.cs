using Microsoft.AspNetCore.Mvc;
using Takwene.API.Authorization;
using Takwene.Application.Common;
using Takwene.Application.DTOs.Tracks;
using Takwene.Application.Interfaces;

namespace Takwene.API.Controllers;

// Artists only ever see and change their own tracks; the service enforces it in every query.
[Route("api/tracks")]
public sealed class TracksController(ITrackService trackService, ITrackDistributionService distributionService) : ApiControllerBase
{
    [HttpPost]
    [EndpointName("CreateTrack")]
    [HasPermission(Permissions.Tracks.Create)]
    [ProducesResponseType<TrackDetailsDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TrackDetailsDto>> Create(CreateTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await trackService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpGet]
    [EndpointName("GetTracks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HasPermission(Permissions.Tracks.Read)]
    public async Task<ActionResult<PagedResult<TrackListItemDto>>> GetAll([FromQuery] TrackQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await trackService.GetAllAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("{id:guid}")]
    [EndpointName("GetTrack")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HasPermission(Permissions.Tracks.Read)]
    public async Task<ActionResult<TrackDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await trackService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPost("{id:guid}/distribute")]
    [EndpointName("DistributeTrack")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HasPermission(Permissions.Tracks.Distribute)]
    public async Task<ActionResult<DistributeTrackResponse>> Distribute(Guid id, DistributeTrackRequest request, CancellationToken cancellationToken)
    {
        var result = await distributionService.DistributeAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPatch("{id:guid}/status")]
    [EndpointName("UpdateTrackStatus")]
    [HasPermission(Permissions.Tracks.UpdateStatus)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTrackStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await trackService.UpdateStatusAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    // Admin only: records the DSP's decision (Pending -> Live or Rejected).
    [HttpPatch("{id:guid}/distributions/{distributionId:guid}/status")]
    [EndpointName("UpdateDistributionStatus")]
    [HasPermission(Permissions.Distributions.UpdateStatus)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateDistributionStatus(Guid id, Guid distributionId, UpdateDistributionStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await distributionService.UpdateDistributionStatusAsync(id, distributionId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }
}

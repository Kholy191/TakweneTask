using Microsoft.AspNetCore.Mvc;
using Takwene.API.Authorization;
using Takwene.Application.Common;
using Takwene.Application.DTOs.Artists;
using Takwene.Application.Interfaces;

namespace Takwene.API.Controllers;

[Route("api/artists")]
public sealed class ArtistsController(IArtistService artistService) : ApiControllerBase
{
    // Creates the artist and its login together; returns a temporary password when none was sent.
    [HttpPost]
    [EndpointName("CreateArtist")]
    [HasPermission(Permissions.Artists.Create)]
    [ProducesResponseType<ArtistCreatedResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ArtistCreatedResponse>> Create(CreateArtistRequest request, CancellationToken cancellationToken)
    {
        var result = await artistService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : HandleFailure(result.Error);
    }

    [HttpGet]
    [EndpointName("GetArtists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HasPermission(Permissions.Artists.Read)]
    public async Task<ActionResult<PagedResult<ArtistDto>>> GetAll([FromQuery] PagedQuery query, CancellationToken cancellationToken) =>
        Ok(await artistService.GetAllAsync(query, cancellationToken));
}

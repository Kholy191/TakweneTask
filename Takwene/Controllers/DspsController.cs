using Microsoft.AspNetCore.Mvc;
using Takwene.API.Authorization;
using Takwene.Application.Common;
using Takwene.Application.DTOs.Dsps;
using Takwene.Application.Interfaces;

namespace Takwene.API.Controllers;

[Route("api/dsps")]
public sealed class DspsController(IDspService dspService) : ApiControllerBase
{
    // Feeds the distribution dropdown in the front-end.
    [HttpGet]
    [EndpointName("GetDsps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HasPermission(Permissions.Dsps.Read)]
    public async Task<ActionResult<IReadOnlyList<DspDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dspService.GetAllAsync(cancellationToken));
}

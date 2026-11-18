using Microsoft.AspNetCore.Mvc;
using Takwene.Application.Common;

namespace Takwene.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // Maps a failed Result's error to a ProblemDetails response with the matching status code.
    protected IActionResult HandleFailure(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(statusCode: statusCode, title: error.Code, detail: error.Description);
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Takwene.Infrastructure.Persistence;

// Two requests racing past the "already exists" check hit the unique index; report it as 409.
internal sealed class UniqueViolationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<UniqueViolationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres })
            return false;

        logger.LogWarning("Unique constraint {Constraint} violated", postgres.ConstraintName);

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Database.Conflict",
                Detail = "The record conflicts with an existing one. Refresh and try again."
            }
        });
    }
}

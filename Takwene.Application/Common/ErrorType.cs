namespace Takwene.Application.Common;

// Drives the HTTP status code the API returns for a failed result.
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

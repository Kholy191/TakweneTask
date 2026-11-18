using FluentValidation;
using Takwene.Application.Common;

namespace Takwene.Application.Artists;

public sealed record CreateArtistRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    // Optional; a temporary password is generated and returned once when omitted.
    public string? Password { get; init; }
}

public sealed record ArtistDto(Guid Id, string Name, string Email, string Country, string? UserId, int TrackCount);

public sealed record ArtistCreatedResponse(Guid Id, string Name, string Email, string Country, string UserId, string? TemporaryPassword);

public static class ArtistErrors
{
    public static readonly Error NotFound = Error.NotFound("Artists.NotFound", "Artist not found.");

    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Artists.EmailAlreadyExists", "An artist with this email already exists.");
}

public sealed class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);

        // Identity enforces the full policy (digit, upper, lower, symbol).
        RuleFor(x => x.Password).MinimumLength(8).When(x => x.Password is not null);
    }
}

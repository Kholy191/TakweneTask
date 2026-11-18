using FluentValidation;
using Takwene.Application.DTOs.Artists;

namespace Takwene.Application.Validators;

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

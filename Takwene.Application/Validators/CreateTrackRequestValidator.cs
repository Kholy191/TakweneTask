using FluentValidation;
using Takwene.Application.DTOs.Tracks;
using Takwene.Domain.Rules;

namespace Takwene.Application.Validators;

public sealed class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReleaseDate).NotEmpty();

        RuleFor(x => x.Isrc)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(isrc => Isrc.IsValid(Isrc.Normalize(isrc)))
            .WithMessage("ISRC must look like CC-XXX-YY-NNNNN (e.g. EG-TKW-26-00001).");

        // Whether ArtistId is required depends on the caller's role, so that check lives in the service.
        RuleFor(x => x.ArtistId).NotEqual(Guid.Empty).When(x => x.ArtistId.HasValue);
    }
}

using FluentValidation;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Validators;

public sealed class TrackQueryParametersValidator : AbstractValidator<TrackQueryParameters>
{
    public const int MinSearchLength = 3;

    public TrackQueryParametersValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Genre).MaximumLength(50);

        // Trigram indexes need at least 3 characters to narrow the search.
        RuleFor(x => x.Search)
            .Must(search => search!.Trim().Length >= MinSearchLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage($"Search must be at least {MinSearchLength} characters.");
    }
}

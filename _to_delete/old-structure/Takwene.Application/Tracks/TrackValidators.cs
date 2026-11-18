using FluentValidation;
using Takwene.Application.Common;
using Takwene.Domain.Rules;

namespace Takwene.Application.Tracks;

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

public sealed class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator()
    {
        RuleFor(x => x.DspIds).NotEmpty();
        RuleForEach(x => x.DspIds).NotEqual(Guid.Empty);
    }
}

public sealed class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateDistributionStatusRequestValidator : AbstractValidator<UpdateDistributionStatusRequest>
{
    public UpdateDistributionStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

using FluentValidation;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Validators;

public sealed class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator()
    {
        RuleFor(x => x.DspIds).NotEmpty();
        RuleForEach(x => x.DspIds).NotEqual(Guid.Empty);
    }
}

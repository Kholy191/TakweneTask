using FluentValidation;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Validators;

public sealed class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

using FluentValidation;
using Takwene.Application.DTOs.Tracks;

namespace Takwene.Application.Validators;

public sealed class UpdateDistributionStatusRequestValidator : AbstractValidator<UpdateDistributionStatusRequest>
{
    public UpdateDistributionStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

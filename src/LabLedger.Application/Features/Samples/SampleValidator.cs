using FluentValidation;

namespace LabLedger.Application.Features.Samples;

public class SampleValidator : AbstractValidator<CreateSampleRequest>
{
    public SampleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Origin)
            .NotEmpty()
            .MaximumLength(200);
    }
}

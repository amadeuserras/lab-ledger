using FluentValidation;

namespace LabLedger.Application.Features.Results;

public class ResultValidator : AbstractValidator<CreateResultRequest>
{
    public ResultValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}

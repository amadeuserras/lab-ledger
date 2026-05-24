using FluentValidation;

namespace LabLedger.Application.Features.Tests;

public class TestValidator : AbstractValidator<CreateTestRequest>
{
    public TestValidator()
    {
        RuleFor(x => x.Method)
            .NotEmpty()
            .MaximumLength(200);
    }
}

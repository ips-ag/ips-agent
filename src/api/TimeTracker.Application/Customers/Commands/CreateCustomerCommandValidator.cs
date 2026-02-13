using FluentValidation;

namespace TimeTracker.Application.Customers.Commands;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).MaximumLength(200);
        RuleFor(x => x.ContactPhone).MaximumLength(50);
    }
}

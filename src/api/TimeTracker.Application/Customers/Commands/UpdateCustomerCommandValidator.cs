using FluentValidation;

namespace TimeTracker.Application.Customers.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).MaximumLength(200);
        RuleFor(x => x.ContactPhone).MaximumLength(50);
    }
}

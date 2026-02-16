using FluentValidation;

namespace TimeTracker.Application.TimeEntries.Commands;

public class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Hours).InclusiveBetween(0.25m, 24m);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

using FluentValidation;

namespace TimeTracker.Application.TimeEntries.Commands;

public class UpdateTimeEntryCommandValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Hours).InclusiveBetween(0.25m, 24m);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

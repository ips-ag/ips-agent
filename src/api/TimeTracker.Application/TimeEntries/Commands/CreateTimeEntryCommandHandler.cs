using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.TimeEntries.Commands;

public class CreateTimeEntryCommandHandler : IRequestHandler<CreateTimeEntryCommand, Guid>
{
    private readonly IRepository<TimeEntry> _timeEntryRepository;
    private readonly IRepository<ProjectTask> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTimeEntryCommandHandler(
        IRepository<TimeEntry> timeEntryRepository,
        IRepository<ProjectTask> taskRepository,
        IUnitOfWork unitOfWork)
    {
        _timeEntryRepository = timeEntryRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTimeEntryCommand request, CancellationToken ct)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, ct)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        if (!task.IsActive)
        {
            throw new InvalidOperationException($"Task '{task.Name}' is not active.");
        }

        var entity = new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TaskId = request.TaskId,
            Date = request.Date,
            Hours = request.Hours,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _timeEntryRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return entity.Id;
    }
}

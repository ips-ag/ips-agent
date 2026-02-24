using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Commands;

public class AssignUserToTaskCommandHandler : IRequestHandler<AssignUserToTaskCommand, MediatR.Unit>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ProjectTask> _taskRepository;
    private readonly IRepository<TaskUser> _taskUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToTaskCommandHandler(
        IRepository<User> userRepository,
        IRepository<ProjectTask> taskRepository,
        IRepository<TaskUser> taskUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _taskUserRepository = taskUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(AssignUserToTaskCommand request, CancellationToken ct)
    {
        _ = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        _ = await _taskRepository.GetByIdAsync(request.TaskId, ct)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        var existing = await _taskUserRepository.Query()
            .FirstOrDefaultAsync(tu => tu.UserId == request.UserId && tu.TaskId == request.TaskId, ct);

        if (existing != null)
        {
            return MediatR.Unit.Value; // Already assigned
        }

        var entity = new TaskUser
        {
            UserId = request.UserId,
            TaskId = request.TaskId,
            AssignedAt = DateTimeOffset.UtcNow
        };

        await _taskUserRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Commands;

public class RemoveUserFromTaskCommandHandler : IRequestHandler<RemoveUserFromTaskCommand, MediatR.Unit>
{
    private readonly IRepository<TaskUser> _taskUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromTaskCommandHandler(
        IRepository<TaskUser> taskUserRepository,
        IUnitOfWork unitOfWork)
    {
        _taskUserRepository = taskUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(RemoveUserFromTaskCommand request, CancellationToken ct)
    {
        var entity = await _taskUserRepository.Query()
            .FirstOrDefaultAsync(tu => tu.UserId == request.UserId && tu.TaskId == request.TaskId, ct)
            ?? throw new NotFoundException(nameof(TaskUser), $"UserId={request.UserId}, TaskId={request.TaskId}");

        await _taskUserRepository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Projects.Commands;

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, MediatR.Unit>
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<ProjectTask> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveProjectCommandHandler(
        IRepository<Project> projectRepository,
        IRepository<ProjectTask> taskRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(ArchiveProjectCommand request, CancellationToken ct)
    {
        var entity = await _projectRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        entity.IsActive = false;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _projectRepository.UpdateAsync(entity, ct);

        // Cascade to child projects
        var childProjects = await _projectRepository.Query()
            .Where(p => p.ParentId == request.Id && p.IsActive)
            .ToListAsync(ct);

        foreach (var child in childProjects)
        {
            child.IsActive = false;
            child.UpdatedAt = DateTimeOffset.UtcNow;
            await _projectRepository.UpdateAsync(child, ct);
        }

        // Cascade to tasks
        var tasks = await _taskRepository.Query()
            .Where(t => t.ProjectId == request.Id && t.IsActive)
            .ToListAsync(ct);

        foreach (var task in tasks)
        {
            task.IsActive = false;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            await _taskRepository.UpdateAsync(task, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return MediatR.Unit.Value;
    }
}

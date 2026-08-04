using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetTasksByUserQueryHandler : IRequestHandler<GetTasksByUserQuery, List<TaskDto>>
{
    private readonly IRepository<TaskUser> _taskUserRepository;

    public GetTasksByUserQueryHandler(IRepository<TaskUser> taskUserRepository)
    {
        _taskUserRepository = taskUserRepository;
    }

    public async Task<List<TaskDto>> Handle(GetTasksByUserQuery request, CancellationToken ct)
    {
        var tasks = await _taskUserRepository.Query()
            .Where(tu => tu.UserId == request.UserId)
            .Include(tu => tu.Task)
                .ThenInclude(t => t.Project)
            .Select(tu => tu.Task)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return tasks.Select(TaskDto.FromEntity).ToList();
    }
}

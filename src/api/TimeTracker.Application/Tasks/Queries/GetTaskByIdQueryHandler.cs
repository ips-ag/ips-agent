using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Tasks.Queries;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly IRepository<ProjectTask> _repository;

    public GetTaskByIdQueryHandler(IRepository<ProjectTask> repository)
    {
        _repository = repository;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProjectTask), request.Id);

        return TaskDto.FromEntity(entity);
    }
}

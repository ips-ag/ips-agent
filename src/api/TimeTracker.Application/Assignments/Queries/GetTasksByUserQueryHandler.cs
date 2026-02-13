using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetTasksByUserQueryHandler : IRequestHandler<GetTasksByUserQuery, List<TaskDto>>
{
    private readonly IRepository<TaskUser> _taskUserRepository;
    private readonly IMapper _mapper;

    public GetTasksByUserQueryHandler(IRepository<TaskUser> taskUserRepository, IMapper mapper)
    {
        _taskUserRepository = taskUserRepository;
        _mapper = mapper;
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

        return _mapper.Map<List<TaskDto>>(tasks);
    }
}

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetUsersByTaskQueryHandler : IRequestHandler<GetUsersByTaskQuery, List<UserDto>>
{
    private readonly IRepository<TaskUser> _taskUserRepository;
    private readonly IMapper _mapper;

    public GetUsersByTaskQueryHandler(IRepository<TaskUser> taskUserRepository, IMapper mapper)
    {
        _taskUserRepository = taskUserRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersByTaskQuery request, CancellationToken ct)
    {
        var users = await _taskUserRepository.Query()
            .Where(tu => tu.TaskId == request.TaskId)
            .Include(tu => tu.User)
            .Select(tu => tu.User)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        return _mapper.Map<List<UserDto>>(users);
    }
}

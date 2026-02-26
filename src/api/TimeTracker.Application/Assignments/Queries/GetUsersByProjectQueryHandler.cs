using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetUsersByProjectQueryHandler : IRequestHandler<GetUsersByProjectQuery, List<UserDto>>
{
    private readonly IRepository<ProjectUser> _projectUserRepository;
    private readonly IMapper _mapper;

    public GetUsersByProjectQueryHandler(IRepository<ProjectUser> projectUserRepository, IMapper mapper)
    {
        _projectUserRepository = projectUserRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersByProjectQuery request, CancellationToken ct)
    {
        var users = await _projectUserRepository.Query()
            .Where(pu => pu.ProjectId == request.ProjectId)
            .Include(pu => pu.User)
            .Select(pu => pu.User)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        return _mapper.Map<List<UserDto>>(users);
    }
}

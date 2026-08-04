using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetUsersByProjectQueryHandler : IRequestHandler<GetUsersByProjectQuery, List<UserDto>>
{
    private readonly IRepository<ProjectUser> _projectUserRepository;

    public GetUsersByProjectQueryHandler(IRepository<ProjectUser> projectUserRepository)
    {
        _projectUserRepository = projectUserRepository;
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

        return users.Select(UserDto.FromEntity).ToList();
    }
}

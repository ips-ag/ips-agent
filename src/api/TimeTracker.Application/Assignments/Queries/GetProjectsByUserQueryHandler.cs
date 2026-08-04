using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetProjectsByUserQueryHandler : IRequestHandler<GetProjectsByUserQuery, List<ProjectDto>>
{
    private readonly IRepository<ProjectUser> _projectUserRepository;

    public GetProjectsByUserQueryHandler(IRepository<ProjectUser> projectUserRepository)
    {
        _projectUserRepository = projectUserRepository;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsByUserQuery request, CancellationToken ct)
    {
        var projects = await _projectUserRepository.Query()
            .Where(pu => pu.UserId == request.UserId)
            .Include(pu => pu.Project)
                .ThenInclude(p => p.Customer)
            .Select(pu => pu.Project)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return projects.Select(ProjectDto.FromEntity).ToList();
    }
}

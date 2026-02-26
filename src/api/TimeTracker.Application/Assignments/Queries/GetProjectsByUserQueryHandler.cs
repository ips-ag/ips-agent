using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Assignments.Queries;

public class GetProjectsByUserQueryHandler : IRequestHandler<GetProjectsByUserQuery, List<ProjectDto>>
{
    private readonly IRepository<ProjectUser> _projectUserRepository;
    private readonly IMapper _mapper;

    public GetProjectsByUserQueryHandler(IRepository<ProjectUser> projectUserRepository, IMapper mapper)
    {
        _projectUserRepository = projectUserRepository;
        _mapper = mapper;
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

        return _mapper.Map<List<ProjectDto>>(projects);
    }
}

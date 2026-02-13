using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Projects.Queries;

public class GetProjectHierarchyQueryHandler : IRequestHandler<GetProjectHierarchyQuery, ProjectDto>
{
    private readonly IRepository<Project> _repository;
    private readonly IMapper _mapper;

    public GetProjectHierarchyQueryHandler(IRepository<Project> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(GetProjectHierarchyQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        var dto = _mapper.Map<ProjectDto>(entity);
        dto.Children = await LoadChildrenAsync(request.Id, ct);
        return dto;
    }

    private async Task<List<ProjectDto>> LoadChildrenAsync(Guid parentId, CancellationToken ct)
    {
        var children = await _repository.Query()
            .Include(p => p.Customer)
            .Where(p => p.ParentId == parentId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        var result = new List<ProjectDto>();
        foreach (var child in children)
        {
            var childDto = _mapper.Map<ProjectDto>(child);
            childDto.Children = await LoadChildrenAsync(child.Id, ct);
            result.Add(childDto);
        }

        return result;
    }
}

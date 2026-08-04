using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Projects.Queries;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IRepository<Project> _repository;

    public GetProjectByIdQueryHandler(IRepository<Project> repository)
    {
        _repository = repository;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Project), request.Id);

        return ProjectDto.FromEntity(entity);
    }
}

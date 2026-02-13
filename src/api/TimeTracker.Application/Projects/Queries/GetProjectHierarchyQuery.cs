using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Projects.Queries;

public record GetProjectHierarchyQuery(Guid Id) : IRequest<ProjectDto>;

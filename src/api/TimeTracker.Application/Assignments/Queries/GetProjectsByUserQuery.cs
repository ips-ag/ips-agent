using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetProjectsByUserQuery(Guid UserId) : IRequest<List<ProjectDto>>;

using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetProjectsByUserQuery(string UserId) : IRequest<List<ProjectDto>>;

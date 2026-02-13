using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetUsersByProjectQuery(Guid ProjectId) : IRequest<List<UserDto>>;

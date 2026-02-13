using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetUsersByTaskQuery(Guid TaskId) : IRequest<List<UserDto>>;

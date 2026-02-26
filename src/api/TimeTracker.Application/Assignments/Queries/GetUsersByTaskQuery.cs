using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetUsersByTaskQuery(string TaskId) : IRequest<List<UserDto>>;

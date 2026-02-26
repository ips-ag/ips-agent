using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Assignments.Queries;

public record GetUsersByProjectQuery(string ProjectId) : IRequest<List<UserDto>>;

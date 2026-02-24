using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Users.Queries;

public record GetUserByIdQuery(string Id) : IRequest<UserDto>;

using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Users.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<PagedList<UserDto>>;

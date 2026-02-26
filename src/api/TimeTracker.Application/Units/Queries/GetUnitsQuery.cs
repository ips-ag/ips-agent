using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Units.Queries;

public record GetUnitsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<PagedList<UnitDto>>;

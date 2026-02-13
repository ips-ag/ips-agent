using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Customers.Queries;

public record GetCustomersQuery(int Page = 1, int PageSize = 20, string? Search = null, Guid? UnitId = null) : IRequest<PagedList<CustomerDto>>;

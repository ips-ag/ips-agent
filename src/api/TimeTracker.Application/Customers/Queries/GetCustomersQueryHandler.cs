using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Customers.Queries;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedList<CustomerDto>>
{
    private readonly IRepository<Customer> _repository;

    public GetCustomersQueryHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Include(c => c.Unit)
            .AsQueryable();

        if (request.UnitId != null)
        {
            query = query.Where(c => c.UnitId == request.UnitId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Name.Contains(request.Search));
        }

        query = query.OrderBy(c => c.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(CustomerDto.Projection)
            .ToListAsync(ct);

        return new PagedList<CustomerDto>(items, totalCount, request.Page, request.PageSize);
    }
}

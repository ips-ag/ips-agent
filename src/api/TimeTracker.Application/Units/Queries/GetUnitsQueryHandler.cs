using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Queries;

public class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, PagedList<UnitDto>>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;
    private readonly IMapper _mapper;

    public GetUnitsQueryHandler(IRepository<Domain.Entities.Unit> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<UnitDto>> Handle(GetUnitsQuery request, CancellationToken ct)
    {
        var query = _repository.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u => u.Name.Contains(request.Search));
        }

        query = query.OrderBy(u => u.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UnitDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedList<UnitDto>(items, totalCount, request.Page, request.PageSize);
    }
}

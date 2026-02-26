using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Projects.Queries;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedList<ProjectDto>>
{
    private readonly IRepository<Project> _repository;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(IRepository<Project> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Include(p => p.Customer)
            .AsQueryable();

        if (request.CustomerId != null)
        {
            query = query.Where(p => p.CustomerId == request.CustomerId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) || p.Code.Contains(request.Search));
        }

        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedList<ProjectDto>(items, totalCount, request.Page, request.PageSize);
    }
}

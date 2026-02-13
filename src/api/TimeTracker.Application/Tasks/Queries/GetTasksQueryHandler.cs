using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Tasks.Queries;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedList<TaskDto>>
{
    private readonly IRepository<ProjectTask> _repository;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(IRepository<ProjectTask> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<TaskDto>> Handle(GetTasksQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Include(t => t.Project)
            .AsQueryable();

        if (request.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search) || t.Code.Contains(request.Search));
        }

        query = query.OrderBy(t => t.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedList<TaskDto>(items, totalCount, request.Page, request.PageSize);
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.TimeEntries.Queries;

public class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, PagedList<TimeEntryDto>>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IMapper _mapper;

    public GetTimeEntriesQueryHandler(IRepository<TimeEntry> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<TimeEntryDto>> Handle(GetTimeEntriesQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .AsQueryable();

        if (request.UserId != null)
        {
            query = query.Where(te => te.UserId == request.UserId);
        }

        if (request.TaskId != null)
        {
            query = query.Where(te => te.TaskId == request.TaskId);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(te => te.Date >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(te => te.Date <= request.DateTo.Value);
        }

        query = query.OrderByDescending(te => te.Date);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TimeEntryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedList<TimeEntryDto>(items, totalCount, request.Page, request.PageSize);
    }
}

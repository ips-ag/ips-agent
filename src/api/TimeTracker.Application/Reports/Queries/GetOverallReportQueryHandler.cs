using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Reports.Queries;

public class GetOverallReportQueryHandler : IRequestHandler<GetOverallReportQuery, OverallReportDto>
{
    private readonly IRepository<TimeEntry> _timeEntryRepo;

    public GetOverallReportQueryHandler(IRepository<TimeEntry> timeEntryRepo)
    {
        _timeEntryRepo = timeEntryRepo;
    }

    public async Task<OverallReportDto> Handle(GetOverallReportQuery request, CancellationToken ct)
    {
        var query = _timeEntryRepo.Query()
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(te => te.Date >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(te => te.Date <= request.DateTo.Value);

        var entries = await query.ToListAsync(ct);

        return new OverallReportDto
        {
            TotalHours = entries.Sum(e => e.Hours),
            ProjectBreakdown = entries
                .GroupBy(e => e.Task.Project.Name)
                .Select(g => new ProjectBreakdownDto { ProjectName = g.Key, Hours = g.Sum(e => e.Hours) })
                .OrderByDescending(p => p.Hours)
                .ToList(),
            UserBreakdown = entries
                .GroupBy(e => $"{e.User.FirstName} {e.User.LastName}")
                .Select(g => new UserBreakdownDto { UserName = g.Key, Hours = g.Sum(e => e.Hours) })
                .OrderByDescending(u => u.Hours)
                .ToList(),
        };
    }
}

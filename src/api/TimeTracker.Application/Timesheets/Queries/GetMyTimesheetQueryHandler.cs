using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Timesheets.Queries;

public class GetMyTimesheetQueryHandler : IRequestHandler<GetMyTimesheetQuery, TimesheetDto>
{
    private readonly IRepository<TimeEntry> _repository;

    public GetMyTimesheetQueryHandler(IRepository<TimeEntry> repository)
    {
        _repository = repository;
    }

    public async Task<TimesheetDto> Handle(GetMyTimesheetQuery request, CancellationToken ct)
    {
        // Ensure WeekStart is a Monday
        var weekStart = request.WeekStart;
        var weekEnd = weekStart.AddDays(6); // Sunday

        var entries = await _repository.Query()
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == request.UserId && te.Date >= weekStart && te.Date <= weekEnd)
            .OrderBy(te => te.Date)
            .ToListAsync(ct);

        var entryDtos = entries.Select(TimeEntryDto.FromEntity).ToList();

        return new TimesheetDto
        {
            WeekStart = weekStart,
            Entries = entryDtos,
            TotalHours = entries.Sum(e => e.Hours)
        };
    }
}

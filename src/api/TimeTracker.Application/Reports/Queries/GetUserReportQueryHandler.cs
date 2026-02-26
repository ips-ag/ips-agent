using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Reports.Queries;

public class GetUserReportQueryHandler : IRequestHandler<GetUserReportQuery, UserReportDto>
{
    private readonly IRepository<TimeEntry> _timeEntryRepo;
    private readonly IRepository<User> _userRepo;

    public GetUserReportQueryHandler(IRepository<TimeEntry> timeEntryRepo, IRepository<User> userRepo)
    {
        _timeEntryRepo = timeEntryRepo;
        _userRepo = userRepo;
    }

    public async Task<UserReportDto> Handle(GetUserReportQuery request, CancellationToken ct)
    {
        var user = await _userRepo.Query()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var query = _timeEntryRepo.Query()
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == request.UserId);

        if (request.DateFrom.HasValue)
            query = query.Where(te => te.Date >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(te => te.Date <= request.DateTo.Value);

        var entries = await query.ToListAsync(ct);

        return new UserReportDto
        {
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}",
            TotalHours = entries.Sum(e => e.Hours),
            ProjectBreakdown = entries
                .GroupBy(e => e.Task.Project.Name)
                .Select(g => new ProjectBreakdownDto { ProjectName = g.Key, Hours = g.Sum(e => e.Hours) })
                .OrderByDescending(p => p.Hours)
                .ToList(),
        };
    }
}

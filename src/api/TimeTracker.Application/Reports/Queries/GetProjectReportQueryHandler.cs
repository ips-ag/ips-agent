using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Reports.Queries;

public class GetProjectReportQueryHandler : IRequestHandler<GetProjectReportQuery, ProjectReportDto>
{
    private readonly IRepository<TimeEntry> _timeEntryRepo;
    private readonly IRepository<Project> _projectRepo;

    public GetProjectReportQueryHandler(IRepository<TimeEntry> timeEntryRepo, IRepository<Project> projectRepo)
    {
        _timeEntryRepo = timeEntryRepo;
        _projectRepo = projectRepo;
    }

    public async Task<ProjectReportDto> Handle(GetProjectReportQuery request, CancellationToken ct)
    {
        var project = await _projectRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        // Collect all project IDs (this project + all descendants)
        var projectIds = await CollectProjectIdsAsync(request.ProjectId, ct);

        var query = _timeEntryRepo.Query()
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => projectIds.Contains(te.Task.ProjectId));

        if (request.DateFrom.HasValue)
            query = query.Where(te => te.Date >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(te => te.Date <= request.DateTo.Value);

        var entries = await query.ToListAsync(ct);

        return new ProjectReportDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            TotalHours = entries.Sum(e => e.Hours),
            TaskBreakdown = entries
                .GroupBy(e => e.Task.Name)
                .Select(g => new TaskBreakdownDto { TaskName = g.Key, Hours = g.Sum(e => e.Hours) })
                .OrderByDescending(t => t.Hours)
                .ToList(),
            UserBreakdown = entries
                .GroupBy(e => $"{e.User.FirstName} {e.User.LastName}")
                .Select(g => new UserBreakdownDto { UserName = g.Key, Hours = g.Sum(e => e.Hours) })
                .OrderByDescending(u => u.Hours)
                .ToList(),
        };
    }

    private async Task<HashSet<string>> CollectProjectIdsAsync(string rootId, CancellationToken ct)
    {
        var ids = new HashSet<string> { rootId };
        var queue = new Queue<string>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            var childIds = await _projectRepo.Query()
                .Where(p => p.ParentId == parentId)
                .Select(p => p.Id)
                .ToListAsync(ct);

            foreach (var childId in childIds)
            {
                if (ids.Add(childId))
                    queue.Enqueue(childId);
            }
        }

        return ids;
    }
}

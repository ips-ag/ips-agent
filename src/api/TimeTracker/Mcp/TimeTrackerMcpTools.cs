using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using TimeTracker.Application.Assignments.Queries;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.Tasks.Queries;
using TimeTracker.Application.TimeEntries.Commands;
using TimeTracker.Application.TimeEntries.Queries;
using TimeTracker.Application.Timesheets.Queries;

namespace TimeTracker.Mcp;

[McpServerToolType]
public sealed class TimeTrackerMcpTools
{
    [McpServerTool(Name = "get_my_projects")]
    [Description(
        "Returns all active projects assigned to the currently authenticated user. " +
        "Use this to discover which projects the user can log time against. " +
        "Each project includes its ID, name, code, customer name, and active date range.")]
    public async Task<List<ProjectDto>> GetMyProjects(
        ISender sender, ICurrentUserService currentUser, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await sender.Send(new GetProjectsByUserQuery(userId), ct);
    }

    [McpServerTool(Name = "get_project_tasks")]
    [Description(
        "Returns all active tasks under a specific project. " +
        "Time entries are logged against tasks (not projects directly), so use this to resolve " +
        "the correct taskId before creating a time entry. " +
        "Each task includes its ID, name, code, description, and active date range.")]
    public async Task<PagedList<TaskDto>> GetProjectTasks(
        ISender sender,
        [Description("The project ID obtained from get_my_projects")] string projectId,
        CancellationToken ct)
    {
        return await sender.Send(new GetTasksQuery(Page: 1, PageSize: 100, ProjectId: projectId), ct);
    }

    [McpServerTool(Name = "get_my_timesheet")]
    [Description(
        "Returns the authenticated user's timesheet for the week containing the specified date. " +
        "Includes all time entries for that week with task/project names and a total hours sum. " +
        "Use this to check existing entries before creating duplicates or to verify remaining " +
        "capacity for a given day (max 24h/day).")]
    public async Task<TimesheetDto> GetMyTimesheet(
        ISender sender, ICurrentUserService currentUser,
        [Description("Any date within the target week, format YYYY-MM-DD")] string date,
        CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await sender.Send(new GetMyTimesheetQuery(userId, DateOnly.Parse(date)), ct);
    }

    [McpServerTool(Name = "get_my_time_entries")]
    [Description(
        "Returns the authenticated user's time entries filtered by optional date range and/or task. " +
        "Use this to check whether a specific entry already exists before creating a new one, " +
        "or to review logged hours for a specific task or period.")]
    public async Task<PagedList<TimeEntryDto>> GetMyTimeEntries(
        ISender sender, ICurrentUserService currentUser,
        [Description("Filter by task ID (optional)")] string? taskId = null,
        [Description("Inclusive start date, format YYYY-MM-DD (optional)")] string? dateFrom = null,
        [Description("Inclusive end date, format YYYY-MM-DD (optional)")] string? dateTo = null,
        CancellationToken ct = default)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await sender.Send(
            new GetTimeEntriesQuery(
                Page: 1,
                PageSize: 100,
                UserId: userId,
                TaskId: taskId,
                DateFrom: dateFrom is not null ? DateOnly.Parse(dateFrom) : null,
                DateTo: dateTo is not null ? DateOnly.Parse(dateTo) : null),
            ct);
    }

    [McpServerTool(Name = "get_task_details")]
    [Description(
        "Returns detailed information about a specific task, including its project association, " +
        "description, code, and active status. " +
        "Use this to confirm a task is active and valid before creating a time entry.")]
    public async Task<TaskDto> GetTaskDetails(
        ISender sender,
        [Description("The task ID to look up")] string taskId,
        CancellationToken ct)
    {
        return await sender.Send(new GetTaskByIdQuery(taskId), ct);
    }

    [McpServerTool(Name = "create_time_entry")]
    [Description(
        "Creates a new time entry for the authenticated user. The entry is logged against a specific " +
        "task on a specific date. Hours must be between 0.25 and 24.00 in 0.25 increments. " +
        "The total hours for the user on the given date must not exceed 24. " +
        "Returns the ID of the created time entry.")]
    public async Task<string> CreateTimeEntry(
        ISender sender, ICurrentUserService currentUser,
        [Description("Target task ID")] string taskId,
        [Description("The date the work was performed, format YYYY-MM-DD")] string date,
        [Description("Hours worked (0.25–24.00, in 0.25 increments)")] decimal hours,
        [Description("Description of work performed (max 500 chars)")] string description,
        CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        return await sender.Send(
            new CreateTimeEntryCommand(userId, taskId, DateOnly.Parse(date), hours, description), ct);
    }
}

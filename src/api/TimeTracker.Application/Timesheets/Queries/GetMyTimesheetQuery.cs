using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Timesheets.Queries;

public record GetMyTimesheetQuery(string UserId, DateOnly WeekStart) : IRequest<TimesheetDto>;

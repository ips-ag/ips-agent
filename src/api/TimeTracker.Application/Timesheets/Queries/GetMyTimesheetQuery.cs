using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Timesheets.Queries;

public record GetMyTimesheetQuery(Guid UserId, DateOnly WeekStart) : IRequest<TimesheetDto>;

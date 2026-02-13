using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Reports.Queries;

public record GetUserReportQuery(Guid UserId, DateOnly? DateFrom = null, DateOnly? DateTo = null) : IRequest<UserReportDto>;

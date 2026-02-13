using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Reports.Queries;

public record GetOverallReportQuery(DateOnly? DateFrom = null, DateOnly? DateTo = null) : IRequest<OverallReportDto>;

using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Reports.Queries;

public record GetProjectReportQuery(string ProjectId, DateOnly? DateFrom = null, DateOnly? DateTo = null) : IRequest<ProjectReportDto>;

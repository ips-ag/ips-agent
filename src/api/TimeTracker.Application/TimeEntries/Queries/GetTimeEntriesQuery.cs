using MediatR;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.TimeEntries.Queries;

public record GetTimeEntriesQuery(
    int Page = 1,
    int PageSize = 20,
    string? UserId = null,
    string? TaskId = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null) : IRequest<PagedList<TimeEntryDto>>;

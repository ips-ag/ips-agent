using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.TimeEntries.Queries;

public record GetTimeEntryByIdQuery(Guid Id) : IRequest<TimeEntryDto>;

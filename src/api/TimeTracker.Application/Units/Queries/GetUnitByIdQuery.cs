using MediatR;
using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Units.Queries;

public record GetUnitByIdQuery(string Id) : IRequest<UnitDto>;

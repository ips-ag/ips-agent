using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Queries;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;

    public GetUnitByIdQueryHandler(IRepository<Domain.Entities.Unit> repository)
    {
        _repository = repository;
    }

    public async Task<UnitDto> Handle(GetUnitByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        return UnitDto.FromEntity(entity);
    }
}

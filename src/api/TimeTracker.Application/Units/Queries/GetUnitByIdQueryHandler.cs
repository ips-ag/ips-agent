using AutoMapper;
using MediatR;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Units.Queries;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto>
{
    private readonly IRepository<Domain.Entities.Unit> _repository;
    private readonly IMapper _mapper;

    public GetUnitByIdQueryHandler(IRepository<Domain.Entities.Unit> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UnitDto> Handle(GetUnitByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);

        return _mapper.Map<UnitDto>(entity);
    }
}

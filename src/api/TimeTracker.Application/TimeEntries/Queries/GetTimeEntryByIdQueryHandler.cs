using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Exceptions;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.TimeEntries.Queries;

public class GetTimeEntryByIdQueryHandler : IRequestHandler<GetTimeEntryByIdQuery, TimeEntryDto>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IMapper _mapper;

    public GetTimeEntryByIdQueryHandler(IRepository<TimeEntry> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TimeEntryDto> Handle(GetTimeEntryByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.Query()
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(te => te.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(TimeEntry), request.Id);

        return _mapper.Map<TimeEntryDto>(entity);
    }
}

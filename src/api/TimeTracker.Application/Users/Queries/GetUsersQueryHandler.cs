using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Common.Models;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Application.Users.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedList<UserDto>>
{
    private readonly IRepository<User> _repository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IRepository<User> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var query = _repository.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u =>
                u.FirstName.Contains(request.Search) ||
                u.LastName.Contains(request.Search) ||
                u.Email.Contains(request.Search));
        }

        query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedList<UserDto>(items, totalCount, request.Page, request.PageSize);
    }
}

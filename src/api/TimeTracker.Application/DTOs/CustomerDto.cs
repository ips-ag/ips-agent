using AutoMapper;
using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class CustomerDto : IMapFrom<Customer>
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.UnitName, opt => opt.MapFrom(s => s.Unit.Name));
    }
}

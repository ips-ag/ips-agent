using AutoMapper;
using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class ProjectDto : IMapFrom<Project>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ProjectDto>? Children { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Project, ProjectDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.ToString("yyyy-MM-dd")))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.HasValue ? s.EndDate.Value.ToString("yyyy-MM-dd") : null));
    }
}

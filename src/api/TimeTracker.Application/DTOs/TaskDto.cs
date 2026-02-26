using AutoMapper;
using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class TaskDto : IMapFrom<ProjectTask>
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ProjectTask, TaskDto>()
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.HasValue ? s.StartDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.HasValue ? s.EndDate.Value.ToString("yyyy-MM-dd") : null));
    }
}

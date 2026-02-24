using AutoMapper;
using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class TimeEntryDto : IMapFrom<TimeEntry>
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string? TaskName { get; set; }
    public string? ProjectName { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TimeEntry, TimeEntryDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
            .ForMember(d => d.TaskName, opt => opt.MapFrom(s => s.Task.Name))
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Task.Project.Name))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("yyyy-MM-dd")));
    }
}

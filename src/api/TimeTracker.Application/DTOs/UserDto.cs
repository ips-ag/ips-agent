using AutoMapper;
using TimeTracker.Application.Common.Mappings;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));
    }
}

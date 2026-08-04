using System.Linq.Expressions;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public string? UnitName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    internal static Expression<Func<Customer, CustomerDto>> Projection { get; } = c => new CustomerDto
    {
        Id = c.Id,
        UnitId = c.UnitId,
        UnitName = c.Unit.Name,
        Name = c.Name,
        Description = c.Description,
        ContactEmail = c.ContactEmail,
        ContactPhone = c.ContactPhone,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static readonly Func<Customer, CustomerDto> s_compiled = Projection.Compile();

    internal static CustomerDto FromEntity(Customer c) => s_compiled(c);
}

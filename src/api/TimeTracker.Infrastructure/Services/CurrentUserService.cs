using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TimeTracker.Application.Common.Interfaces;

namespace TimeTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId
    {
        get
        {
            return User?.FindFirstValue("sub");
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                            ?? User?.FindFirstValue("preferred_username");

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);
}

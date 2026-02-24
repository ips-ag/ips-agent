using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Interfaces;

namespace TimeTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<User> _userRepository;
    private User? _cachedUser;
    private bool _userLoaded;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IRepository<User> userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public bool IsAuthenticated
    {
        get => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }

    public string? UserId
    {
        get => GetDbUser()?.Id;
    }

    public string? Email
    {
        get => GetDbUser()?.Email;
    }

    public string? Role
    {
        get => GetDbUser()?.Role.ToString();
    }

    private User? GetDbUser()
    {
        if (_userLoaded) return _cachedUser;
        _userLoaded = true;

        string? sub = _httpContextAccessor.HttpContext?.User.FindFirstValue("sub") ??
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null) return null;

        _cachedUser = _userRepository.Query().FirstOrDefault(u => u.ExternalId == sub);
        return _cachedUser;
    }
}

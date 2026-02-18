using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Application.Timesheets.Queries;

namespace TimeTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TimesheetsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;
    public TimesheetsController(ISender sender, ICurrentUserService currentUser) { _sender = sender; _currentUser = currentUser; }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTimesheet([FromQuery] DateOnly week, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _sender.Send(new GetMyTimesheetQuery(userId, week), ct));
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Timesheets.Queries;

namespace TimeTracker.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TimesheetsController : ControllerBase
{
    private readonly ISender _sender;
    public TimesheetsController(ISender sender) => _sender = sender;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTimesheet([FromQuery] DateOnly week, CancellationToken ct)
    {
        // TODO: Replace with actual current user ID from authentication
        var userId = Guid.Empty;
        return Ok(await _sender.Send(new GetMyTimesheetQuery(userId, week), ct));
    }
}

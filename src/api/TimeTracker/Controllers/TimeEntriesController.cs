using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Application.DTOs;
using TimeTracker.Application.TimeEntries.Commands;
using TimeTracker.Application.TimeEntries.Queries;

namespace TimeTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TimeEntriesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;

    public TimeEntriesController(ISender sender, ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] string? taskId = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        return Ok(await _sender.Send(new GetTimeEntriesQuery(page, pageSize, userId, taskId, dateFrom, dateTo), ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        return Ok(await _sender.Send(new GetTimeEntryByIdQuery(id), ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryRequestDto request, CancellationToken ct)
    {
        string userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var command = new CreateTimeEntryCommand(
            UserId: userId,
            TaskId: request.TaskId,
            Date: request.Date,
            Hours: request.Hours,
            Description: request.Description);
        string id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTimeEntryCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _sender.Send(new DeleteTimeEntryCommand(id), ct);
        return NoContent();
    }
}

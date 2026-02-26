using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Assignments.Queries;
using TimeTracker.Application.Users.Commands;
using TimeTracker.Application.Users.Queries;

namespace TimeTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetUsersQuery(page, pageSize, search), ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
        => Ok(await _sender.Send(new GetUserByIdQuery(id), ct));

    [HttpGet("{id}/projects")]
    public async Task<IActionResult> GetProjects(string id, CancellationToken ct)
        => Ok(await _sender.Send(new GetProjectsByUserQuery(id), ct));

    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetTasks(string id, CancellationToken ct)
        => Ok(await _sender.Send(new GetTasksByUserQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] SyncUserCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateUserCommand(id), ct);
        return NoContent();
    }
}

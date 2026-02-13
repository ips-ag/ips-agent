using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Assignments.Commands;
using TimeTracker.Application.Assignments.Queries;
using TimeTracker.Application.Tasks.Commands;
using TimeTracker.Application.Tasks.Queries;

namespace TimeTracker.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;
    public TasksController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? projectId = null, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetTasksQuery(page, pageSize, search, projectId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetTaskByIdQuery(id), ct));

    [HttpGet("{id:guid}/users")]
    public async Task<IActionResult> GetUsers(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetUsersByTaskQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("{id:guid}/users")]
    public async Task<IActionResult> AssignUser(Guid id, [FromBody] AssignUserToTaskRequest request, CancellationToken ct)
    {
        await _sender.Send(new AssignUserToTaskCommand(request.UserId, id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveTaskCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId, CancellationToken ct)
    {
        await _sender.Send(new RemoveUserFromTaskCommand(userId, id), ct);
        return NoContent();
    }

    public record AssignUserToTaskRequest(Guid UserId);
}

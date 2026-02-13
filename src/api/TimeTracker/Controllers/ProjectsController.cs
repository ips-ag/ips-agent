using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Assignments.Commands;
using TimeTracker.Application.Assignments.Queries;
using TimeTracker.Application.Projects.Commands;
using TimeTracker.Application.Projects.Queries;

namespace TimeTracker.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;
    public ProjectsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? customerId = null, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetProjectsQuery(page, pageSize, search, customerId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetProjectByIdQuery(id), ct));

    [HttpGet("{id:guid}/hierarchy")]
    public async Task<IActionResult> GetHierarchy(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetProjectHierarchyQuery(id), ct));

    [HttpGet("{id:guid}/users")]
    public async Task<IActionResult> GetUsers(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetUsersByProjectQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("{id:guid}/users")]
    public async Task<IActionResult> AssignUser(Guid id, [FromBody] AssignUserToProjectRequest request, CancellationToken ct)
    {
        await _sender.Send(new AssignUserToProjectCommand(request.UserId, id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveProjectCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId, CancellationToken ct)
    {
        await _sender.Send(new RemoveUserFromProjectCommand(userId, id), ct);
        return NoContent();
    }

    public record AssignUserToProjectRequest(Guid UserId);
}

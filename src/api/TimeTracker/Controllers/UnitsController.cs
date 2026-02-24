using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Units.Commands;
using TimeTracker.Application.Units.Queries;

namespace TimeTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly ISender _sender;
    public UnitsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetUnitsQuery(page, pageSize, search), ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
        => Ok(await _sender.Send(new GetUnitByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUnitCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Archive(string id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveUnitCommand(id), ct);
        return NoContent();
    }
}

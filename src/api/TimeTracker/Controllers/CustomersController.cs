using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Customers.Commands;
using TimeTracker.Application.Customers.Queries;

namespace TimeTracker.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;
    public CustomersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? unitId = null, CancellationToken ct = default)
        => Ok(await _sender.Send(new GetCustomersQuery(page, pageSize, search, unitId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetCustomerByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ArchiveCustomerCommand(id), ct);
        return NoContent();
    }
}

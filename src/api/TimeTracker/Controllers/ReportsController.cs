using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Application.Reports.Queries;

namespace TimeTracker.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;
    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("project/{id:guid}")]
    public async Task<IActionResult> GetProjectReport(
        Guid id,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        CancellationToken ct = default)
    {
        var from = dateFrom is not null ? DateOnly.Parse(dateFrom) : (DateOnly?)null;
        var to = dateTo is not null ? DateOnly.Parse(dateTo) : (DateOnly?)null;
        return Ok(await _sender.Send(new GetProjectReportQuery(id, from, to), ct));
    }

    [HttpGet("user/{id:guid}")]
    public async Task<IActionResult> GetUserReport(
        Guid id,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        CancellationToken ct = default)
    {
        var from = dateFrom is not null ? DateOnly.Parse(dateFrom) : (DateOnly?)null;
        var to = dateTo is not null ? DateOnly.Parse(dateTo) : (DateOnly?)null;
        return Ok(await _sender.Send(new GetUserReportQuery(id, from, to), ct));
    }

    [HttpGet("overall")]
    public async Task<IActionResult> GetOverallReport(
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        CancellationToken ct = default)
    {
        var from = dateFrom is not null ? DateOnly.Parse(dateFrom) : (DateOnly?)null;
        var to = dateTo is not null ? DateOnly.Parse(dateTo) : (DateOnly?)null;
        return Ok(await _sender.Send(new GetOverallReportQuery(from, to), ct));
    }
}

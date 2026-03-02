using Atlas.Staff.Application.StaffMembers.Commands.Create;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] Command command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Query query,
        CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
using FrClassifier.DTOs;
using FrClassifier.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrClassifier.Controllers;

[ApiController]
[Route("api/classifications")]
public sealed class ClassificationsController(ClassificationDispatchService dispatchService) : ControllerBase
{
    [HttpPost("dispatch")]
    [ProducesResponseType<ClassificationDispatchResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClassificationDispatchResponse>> DispatchPending(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 500)
        {
            return BadRequest("limit must be between 1 and 500.");
        }

        return Ok(await dispatchService.DispatchPendingAsync(limit, cancellationToken));
    }
}
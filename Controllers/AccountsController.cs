using FrClassifier.DTOs;
using FrClassifier.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FrClassifier.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(IDocumentRepository documents) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var account = await documents.GetAccountAsync(id, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        return Ok(AccountResponse.From(account));
    }
}
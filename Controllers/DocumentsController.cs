using FrClassifier.DTOs;
using FrClassifier.Entities;
using FrClassifier.Repositories;
using FrClassifier.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrClassifier.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(
    ImportDocumentService importService,
    IDocumentRepository documents,
    IDocumentStorage storage,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<DocumentImportResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<DocumentImportResponse>> Upload(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("Multipart field 'file' is required.");
        }

        var maxUploadBytes = configuration.GetValue("Documents:MaxUploadBytes", 25_000_000L);
        if (file.Length == 0)
        {
            return BadRequest("The uploaded document is empty.");
        }

        if (file.Length > maxUploadBytes)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, "The uploaded document exceeds the size limit.");
        }

        try
        {
            await using var content = file.OpenReadStream();
            var document = await importService.ImportAsync(
                file.FileName,
                file.ContentType,
                file.Length,
                content,
                cancellationToken);
            var response = new DocumentImportResponse(
                document.Id,
                document.FileName,
                await documents.CountAccountsAsync(document.Id, cancellationToken),
                document.ImportStatus.ToString());
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, response);
        }
        catch (NotSupportedException exception)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, exception.Message);
        }
        catch (InvalidDataException exception)
        {
            return UnprocessableEntity(new ProblemDetails { Detail = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<DocumentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DocumentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document = await documents.GetAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        return Ok(DocumentResponse.From(
            document,
            await documents.CountAccountsAsync(document.Id, cancellationToken)));
    }

    [HttpGet("{id:guid}/accounts")]
    [ProducesResponseType<IReadOnlyList<AccountResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> GetAccounts(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 500)
        {
            return BadRequest("page must be positive and pageSize must be between 1 and 500.");
        }

        if (await documents.GetAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        var accounts = await documents.GetAccountsAsync(
            id,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        return Ok(accounts.Select(AccountResponse.From).ToArray());
    }

    [HttpGet("{id:guid}/content")]
    public async Task<IActionResult> GetOriginalFile(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        var content = await storage.OpenReadAsync(document.StorageKey, cancellationToken);
        return File(content, document.ContentType, document.FileName, enableRangeProcessing: true);
    }
}
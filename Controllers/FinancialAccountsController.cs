using FrClassifier.DTOs;
using FrClassifier.Entities;
using FrClassifier.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FrClassifier.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class FinancialAccountsController(IFinancialDocumentRepository documents) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FinancialAccountResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialAccountResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var account = await documents.GetAccountAsync(id, cancellationToken);
        if (account is null)
        {
            return NotFound();
        }

        var classification = account.Classifications.FirstOrDefault();
        return Ok(new FinancialAccountResponse(
            account.Id,
            account.FinancialDocumentId,
            account.CompanyCode,
            account.Ledger,
            account.FiscalYear,
            account.AccountingDocumentNumber,
            account.LedgerLineNumber,
            account.GLAccount,
            account.GLAccountName,
            account.LineDescription,
            account.PostingDate,
            account.DocumentDate,
            account.AmountInTransactionCurrency,
            account.TransactionCurrencyCode,
            account.AmountInCompanyCodeCurrency,
            account.CompanyCodeCurrencyCode,
            account.ProfitCenter,
            account.CostCenter,
            account.Segment,
            account.SourceWorksheet,
            account.SourceRowNumber,
            account.ClassificationStatus.ToString(),
            classification is null ? null : IFRS18CategoryContract.ToCode(classification.Category),
            classification?.Confidence,
            account.ClassifiedAt));
    }
}

[ApiController]
[Route("api/ifrs18-categories")]
public sealed class IFRS18CategoriesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<IFRS18CategoryDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<IFRS18CategoryDto>> Get() => Ok(IFRS18CategoryContract.All);
}
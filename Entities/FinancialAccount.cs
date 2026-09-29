namespace FrClassifier.Entities;

public sealed class FinancialAccount
{
    public Guid Id { get; set; }
    public Guid FinancialDocumentId { get; set; }
    public FinancialDocument FinancialDocument { get; set; } = null!;

    public required string CompanyCode { get; set; }
    public string Ledger { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public required string AccountingDocumentNumber { get; set; }
    public required string LedgerLineNumber { get; set; }
    public required string GLAccount { get; set; }
    public string? GLAccountName { get; set; }
    public string? LineDescription { get; set; }
    public DateOnly PostingDate { get; set; }
    public DateOnly? DocumentDate { get; set; }
    public decimal AmountInTransactionCurrency { get; set; }
    public required string TransactionCurrencyCode { get; set; }
    public decimal? AmountInCompanyCodeCurrency { get; set; }
    public string? CompanyCodeCurrencyCode { get; set; }
    public string? ProfitCenter { get; set; }
    public string? CostCenter { get; set; }
    public string? Segment { get; set; }

    public required string SourceWorksheet { get; set; }
    public int SourceRowNumber { get; set; }
    public required string SourceRowHash { get; set; }
    public ClassificationStatus ClassificationStatus { get; set; } = ClassificationStatus.Pending;
    public Guid? ClassificationRequestId { get; set; }
    public DateTimeOffset? ClassificationRequestedAt { get; set; }
    public DateTimeOffset? ClassifiedAt { get; set; }

    public ICollection<FinancialAccountClassification> Classifications { get; set; } =
        new List<FinancialAccountClassification>();
}
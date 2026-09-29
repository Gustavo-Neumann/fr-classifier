namespace FrClassifier.DTOs;

public sealed record FinancialDocumentResponse(
    Guid Id,
    string FileName,
    string Sha256,
    string ImportStatus,
    int AccountCount,
    DateTimeOffset UploadedAt);

public sealed record FinancialAccountResponse(
    Guid Id,
    Guid FinancialDocumentId,
    string CompanyCode,
    string Ledger,
    int FiscalYear,
    string AccountingDocumentNumber,
    string LedgerLineNumber,
    string GLAccount,
    string? GLAccountName,
    string? LineDescription,
    DateOnly PostingDate,
    DateOnly? DocumentDate,
    decimal AmountInTransactionCurrency,
    string TransactionCurrencyCode,
    decimal? AmountInCompanyCodeCurrency,
    string? CompanyCodeCurrencyCode,
    string? ProfitCenter,
    string? CostCenter,
    string? Segment,
    string SourceWorksheet,
    int SourceRowNumber,
    string ClassificationStatus,
    string? IFRS18Category,
    decimal? ClassificationConfidence,
    DateTimeOffset? ClassifiedAt);

public sealed record DocumentImportResponse(
    Guid DocumentId,
    string FileName,
    int ImportedAccountCount,
    string ImportStatus);

public sealed record ClassificationDispatchResponse(int QueuedCount, int FailedCount);
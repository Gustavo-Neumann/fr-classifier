namespace FrClassifier.DTOs;

public sealed record ClassificationRequestMessage(
    Guid RequestId,
    Guid FinancialAccountId,
    Guid FinancialDocumentId,
    string Ledger,
    string CompanyCode,
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
    string? Segment);

public sealed record ClassificationResultMessage(
    Guid RequestId,
    Guid FinancialAccountId,
    string Category,
    decimal? Confidence,
    string? Rationale,
    string ClassifierName,
    string? ModelVersion);
namespace FrClassifier.DTOs;

public sealed record ClassificationRequestMessage(
    Guid RequestId,
    Guid AccountId,
    Guid DocumentId,
    string? EntityCode,
    int? FiscalYear,
    string? AccountCode,
    string? AccountName,
    string? Description,
    DateOnly? PostingDate,
    DateOnly? DocumentDate,
    decimal Amount,
    string CurrencyCode,
    decimal? ReportingAmount,
    string? ReportingCurrencyCode,
    string? SourceReference,
    string? DimensionsJson);
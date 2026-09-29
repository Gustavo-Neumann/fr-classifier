using FrClassifier.Entities;

namespace FrClassifier.DTOs;

public sealed record AccountResponse(
    Guid Id,
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
    string? SourceLocation,
    int? SourceRowNumber,
    string? DimensionsJson,
    string ClassificationStatus,
    string? Category,
    decimal? Confidence,
    DateTimeOffset? ClassifiedAt)
{
    public static AccountResponse From(Account account)
    {
        var classification = account.Classifications.FirstOrDefault();
        return new AccountResponse(
            account.Id,
            account.DocumentId,
            account.EntityCode,
            account.FiscalYear,
            account.AccountCode,
            account.AccountName,
            account.Description,
            account.PostingDate,
            account.DocumentDate,
            account.Amount,
            account.CurrencyCode,
            account.ReportingAmount,
            account.ReportingCurrencyCode,
            account.SourceReference,
            account.SourceLocation,
            account.SourceRowNumber,
            account.DimensionsJson,
            account.ClassificationStatus.ToString(),
            classification is null ? null : CategoryContract.ToCode(classification.Category),
            classification?.Confidence,
            account.ClassifiedAt);
    }
}
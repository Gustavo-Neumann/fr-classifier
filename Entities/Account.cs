namespace FrClassifier.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public string? EntityCode { get; set; }
    public int? FiscalYear { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public DateOnly? PostingDate { get; set; }
    public DateOnly? DocumentDate { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public decimal? ReportingAmount { get; set; }
    public string? ReportingCurrencyCode { get; set; }
    public string? SourceReference { get; set; }
    public string? SourceLocation { get; set; }
    public string? DimensionsJson { get; set; }

    public int? SourceRowNumber { get; set; }
    public required string SourceRowHash { get; set; }
    public ClassificationStatus ClassificationStatus { get; set; } = ClassificationStatus.Pending;
    public Guid? ClassificationRequestId { get; set; }
    public DateTimeOffset? ClassificationRequestedAt { get; set; }
    public DateTimeOffset? ClassifiedAt { get; set; }

    public ICollection<AccountClassification> Classifications { get; set; } =
        new List<AccountClassification>();
}
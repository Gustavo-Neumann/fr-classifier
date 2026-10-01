namespace FrClassifier.Entities;

public sealed class AccountClassification
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public Guid RequestId { get; set; }
    public Category? Category { get; set; }
    public decimal? Confidence { get; set; }
    public string? Rationale { get; set; }
    public bool NeedsReview { get; set; }
    public required string ClassifierName { get; set; }
    public string? ModelVersion { get; set; }
    public string? ResultJson { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
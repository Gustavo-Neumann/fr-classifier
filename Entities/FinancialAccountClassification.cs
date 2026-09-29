namespace FrClassifier.Entities;

public sealed class FinancialAccountClassification
{
    public Guid Id { get; set; }
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public Guid RequestId { get; set; }
    public IFRS18Category Category { get; set; }
    public decimal? Confidence { get; set; }
    public string? Rationale { get; set; }
    public required string ClassifierName { get; set; }
    public string? ModelVersion { get; set; }
    public string? ResultJson { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
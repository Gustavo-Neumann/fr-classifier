namespace FrClassifier.DTOs;

public sealed record ClassificationResultMessage(
    Guid RequestId,
    Guid AccountId,
    string Category,
    decimal? Confidence,
    string? Rationale,
    string ClassifierName,
    string? ModelVersion);
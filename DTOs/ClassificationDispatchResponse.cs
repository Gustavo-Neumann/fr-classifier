namespace FrClassifier.DTOs;

public sealed record ClassificationDispatchResponse(
    int QueuedCount, 
    int FailedCount);
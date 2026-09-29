namespace FrClassifier.DTOs;

public sealed record DocumentImportResponse(
    Guid DocumentId,
    string FileName,
    int ImportedAccountCount,
    string ImportStatus);
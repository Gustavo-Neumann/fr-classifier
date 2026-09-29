using FrClassifier.Entities;

namespace FrClassifier.DTOs;

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string Sha256,
    string ImportStatus,
    int AccountCount,
    DateTimeOffset UploadedAt)
{
    public static DocumentResponse From(Document document, int accountCount) => new(
        document.Id,
        document.FileName,
        document.Sha256,
        document.ImportStatus.ToString(),
        accountCount,
        document.UploadedAt);
}
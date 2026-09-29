namespace FrClassifier.Entities;

public sealed class FinancialDocument
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string Sha256 { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public DocumentImportStatus ImportStatus { get; set; } = DocumentImportStatus.Processing;
    public string? ImportError { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<FinancialAccount> Accounts { get; set; } = new List<FinancialAccount>();
}
using System.Security.Cryptography;
using FrClassifier.Entities;
using FrClassifier.Repositories;

namespace FrClassifier.Services;

public sealed class FinancialDocumentImportService(
    IEnumerable<IFinancialDocumentParser> parsers,
    IFinancialDocumentRepository documents,
    IFinancialDocumentStorage storage)
{
    public async Task<FinancialDocument> ImportAsync(
        string fileName,
        string contentType,
        long fileSizeBytes,
        Stream content,
        CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(fileName);
        var parser = parsers.FirstOrDefault(item => item.CanParse(safeFileName, contentType))
            ?? throw new NotSupportedException("This financial document format is not supported.");

        await using var bufferedContent = new MemoryStream();
        await content.CopyToAsync(bufferedContent, cancellationToken);
        if (bufferedContent.Length == 0)
        {
            throw new InvalidDataException("The uploaded document is empty.");
        }
        if (bufferedContent.Length != fileSizeBytes)
        {
            throw new InvalidDataException("The uploaded document size does not match its content.");
        }

        var document = new FinancialDocument
        {
            Id = Guid.NewGuid(),
            FileName = safeFileName,
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType,
            FileSizeBytes = fileSizeBytes,
            Sha256 = Convert.ToHexString(SHA256.HashData(bufferedContent.ToArray())).ToLowerInvariant(),
            ImportStatus = DocumentImportStatus.Processing
        };
        bufferedContent.Position = 0;
        document.StorageKey = await storage.StoreAsync(
            document.Id,
            safeFileName,
            bufferedContent,
            cancellationToken);

        await documents.CreateAsync(document, cancellationToken);
        try
        {
            bufferedContent.Position = 0;
            var accounts = await parser.ParseAsync(bufferedContent, document.Id, cancellationToken);
            await documents.CompleteImportAsync(document, accounts, cancellationToken);
            return document;
        }
        catch (Exception exception)
        {
            await documents.MarkImportFailedAsync(
                document.Id,
                $"{exception.GetType().Name}: {exception.Message}",
                CancellationToken.None);
            throw;
        }
    }
}
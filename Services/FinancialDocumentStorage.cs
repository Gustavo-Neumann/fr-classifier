namespace FrClassifier.Services;

public interface IFinancialDocumentStorage
{
    Task<string> StoreAsync(
        Guid documentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
}

public sealed class FinancialDocumentStorage : IFinancialDocumentStorage
{
    private readonly string _rootPath;

    public FinancialDocumentStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["FinancialDocuments:StoragePath"];
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "financial-documents")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);
        _rootPath = Path.GetFullPath(path);
    }

    public async Task<string> StoreAsync(
        Guid documentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_rootPath);
        var extension = Path.GetExtension(fileName);
        var storageKey = $"{documentId:N}{extension}";
        var fullPath = GetPath(storageKey);

        await using var output = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await content.CopyToAsync(output, cancellationToken);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream file = new FileStream(
            GetPath(storageKey),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(file);
    }

    private string GetPath(string storageKey)
    {
        if (Path.GetFileName(storageKey) != storageKey)
        {
            throw new InvalidOperationException("Invalid document storage key.");
        }

        return Path.Combine(_rootPath, storageKey);
    }
}
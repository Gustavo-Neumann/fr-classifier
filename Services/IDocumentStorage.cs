namespace FrClassifier.Services;

public interface IDocumentStorage
{
    Task<string> StoreAsync(
        Guid documentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
}
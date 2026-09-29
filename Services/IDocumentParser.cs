using FrClassifier.Entities;

namespace FrClassifier.Services;

public interface IDocumentParser
{
    bool CanParse(string fileName, string contentType);

    Task<IReadOnlyList<Account>> ParseAsync(
        Stream content,
        Guid documentId,
        CancellationToken cancellationToken);
}
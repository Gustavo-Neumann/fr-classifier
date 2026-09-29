using FrClassifier.Entities;

namespace FrClassifier.Services;

public interface IFinancialDocumentParser
{
    bool CanParse(string fileName, string contentType);

    Task<IReadOnlyList<FinancialAccount>> ParseAsync(
        Stream content,
        Guid financialDocumentId,
        CancellationToken cancellationToken);
}
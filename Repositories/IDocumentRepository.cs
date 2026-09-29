using FrClassifier.Entities;

namespace FrClassifier.Repositories;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken cancellationToken);
    Task CompleteImportAsync(
        Document document,
        IReadOnlyCollection<Account> accounts,
        CancellationToken cancellationToken);
    Task MarkImportFailedAsync(Guid documentId, string error, CancellationToken cancellationToken);
    Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAccountsAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Account>> GetAccountsAsync(
        Guid documentId,
        int skip,
        int take,
        CancellationToken cancellationToken);
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
}
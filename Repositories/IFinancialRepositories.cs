using FrClassifier.Entities;

namespace FrClassifier.Repositories;

public interface IFinancialDocumentRepository
{
    Task CreateAsync(FinancialDocument document, CancellationToken cancellationToken);
    Task CompleteImportAsync(
        FinancialDocument document,
        IReadOnlyCollection<FinancialAccount> accounts,
        CancellationToken cancellationToken);
    Task MarkImportFailedAsync(Guid documentId, string error, CancellationToken cancellationToken);
    Task<FinancialDocument?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAccountsAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FinancialAccount>> GetAccountsAsync(
        Guid documentId,
        int skip,
        int take,
        CancellationToken cancellationToken);
    Task<FinancialAccount?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
}

public interface IFinancialAccountRepository
{
    Task<IReadOnlyList<FinancialAccount>> GetPendingAsync(
        int limit,
        CancellationToken cancellationToken);
    Task<bool> TryQueueAsync(
        Guid accountId,
        Guid requestId,
        DateTimeOffset queuedAt,
        CancellationToken cancellationToken);
    Task ResetPendingAsync(Guid accountId, Guid requestId, CancellationToken cancellationToken);
    Task ApplyClassificationAsync(
        FinancialAccountClassification classification,
        CancellationToken cancellationToken);
}
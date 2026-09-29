using FrClassifier.Entities;

namespace FrClassifier.Repositories;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetPendingAsync(int limit, CancellationToken cancellationToken);
    Task<bool> TryQueueAsync(
        Guid accountId,
        Guid requestId,
        DateTimeOffset queuedAt,
        CancellationToken cancellationToken);
    Task ResetPendingAsync(Guid accountId, Guid requestId, CancellationToken cancellationToken);
    Task ApplyClassificationAsync(
        AccountClassification classification,
        CancellationToken cancellationToken);
}
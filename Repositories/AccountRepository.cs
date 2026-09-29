using FrClassifier.Data;
using FrClassifier.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrClassifier.Repositories;

public sealed class AccountRepository(FrClassifierDbContext dbContext) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetPendingAsync(
        int limit,
        CancellationToken cancellationToken) =>
        await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.ClassificationStatus == ClassificationStatus.Pending)
            .OrderBy(account => account.PostingDate)
            .ThenBy(account => account.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<bool> TryQueueAsync(
        Guid accountId,
        Guid requestId,
        DateTimeOffset queuedAt,
        CancellationToken cancellationToken) =>
        await dbContext.Accounts
            .Where(account => account.Id == accountId
                && account.ClassificationStatus == ClassificationStatus.Pending)
            .ExecuteUpdateAsync(update => update
                .SetProperty(account => account.ClassificationStatus, ClassificationStatus.Queued)
                .SetProperty(account => account.ClassificationRequestId, requestId)
                .SetProperty(account => account.ClassificationRequestedAt, queuedAt), cancellationToken) == 1;

    public async Task ResetPendingAsync(
        Guid accountId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        await dbContext.Accounts
            .Where(account => account.Id == accountId
                && account.ClassificationRequestId == requestId
                && account.ClassificationStatus == ClassificationStatus.Queued)
            .ExecuteUpdateAsync(update => update
                .SetProperty(account => account.ClassificationStatus, ClassificationStatus.Pending)
                .SetProperty(account => account.ClassificationRequestId, (Guid?)null)
                .SetProperty(account => account.ClassificationRequestedAt, (DateTimeOffset?)null),
                cancellationToken);
    }

    public async Task ApplyClassificationAsync(
        AccountClassification classification,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .Include(item => item.Classifications)
            .SingleOrDefaultAsync(item => item.Id == classification.AccountId, cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException("Account was not found.");
        }

        if (account.ClassificationStatus == ClassificationStatus.Classified
            && account.ClassificationRequestId == classification.RequestId)
        {
            return;
        }

        if (account.ClassificationStatus != ClassificationStatus.Queued
            || account.ClassificationRequestId != classification.RequestId)
        {
            throw new InvalidOperationException("The classification result does not match the active request.");
        }

        if (classification.Confidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(classification), "Confidence must be between 0 and 1.");
        }

        account.Classifications.Add(classification);
        account.ClassificationStatus = ClassificationStatus.Classified;
        account.ClassifiedAt = classification.ReceivedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
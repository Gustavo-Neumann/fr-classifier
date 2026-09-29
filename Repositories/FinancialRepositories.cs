using FrClassifier.Data;
using FrClassifier.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrClassifier.Repositories;

public sealed class FinancialDocumentRepository(FrClassifierDbContext dbContext)
    : IFinancialDocumentRepository
{
    public async Task CreateAsync(FinancialDocument document, CancellationToken cancellationToken)
    {
        dbContext.FinancialDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteImportAsync(
        FinancialDocument document,
        IReadOnlyCollection<FinancialAccount> accounts,
        CancellationToken cancellationToken)
    {
        document.ImportStatus = DocumentImportStatus.Imported;
        dbContext.FinancialAccounts.AddRange(accounts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkImportFailedAsync(
        Guid documentId,
        string error,
        CancellationToken cancellationToken)
    {
        await dbContext.FinancialDocuments
            .Where(document => document.Id == documentId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(document => document.ImportStatus, DocumentImportStatus.Failed)
                .SetProperty(document => document.ImportError, error.Length <= 2000 ? error : error[..2000]),
                cancellationToken);
    }

    public Task<FinancialDocument?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.FinancialDocuments
            .AsNoTracking()
            .SingleOrDefaultAsync(document => document.Id == id, cancellationToken);

    public Task<int> CountAccountsAsync(Guid documentId, CancellationToken cancellationToken) =>
        dbContext.FinancialAccounts.CountAsync(
            account => account.FinancialDocumentId == documentId,
            cancellationToken);

    public async Task<IReadOnlyList<FinancialAccount>> GetAccountsAsync(
        Guid documentId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await dbContext.FinancialAccounts
            .AsNoTracking()
            .Where(account => account.FinancialDocumentId == documentId)
            .Include(account => account.Classifications.OrderByDescending(result => result.ReceivedAt))
            .OrderBy(account => account.SourceWorksheet)
            .ThenBy(account => account.SourceRowNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<FinancialAccount?> GetAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        dbContext.FinancialAccounts
            .AsNoTracking()
            .Include(account => account.FinancialDocument)
            .Include(account => account.Classifications.OrderByDescending(result => result.ReceivedAt))
            .SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken);
}

public sealed class FinancialAccountRepository(FrClassifierDbContext dbContext)
    : IFinancialAccountRepository
{
    public async Task<IReadOnlyList<FinancialAccount>> GetPendingAsync(
        int limit,
        CancellationToken cancellationToken) =>
        await dbContext.FinancialAccounts
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
        await dbContext.FinancialAccounts
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
        await dbContext.FinancialAccounts
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
        FinancialAccountClassification classification,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.FinancialAccounts
            .Include(item => item.Classifications)
            .SingleOrDefaultAsync(
                item => item.Id == classification.FinancialAccountId,
                cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException("Financial account was not found.");
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
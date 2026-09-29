using FrClassifier.Data;
using FrClassifier.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrClassifier.Repositories;

public sealed class DocumentRepository(FrClassifierDbContext dbContext)
    : IDocumentRepository
{
    public async Task CreateAsync(Document document, CancellationToken cancellationToken)
    {
        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteImportAsync(
        Document document,
        IReadOnlyCollection<Account> accounts,
        CancellationToken cancellationToken)
    {
        document.ImportStatus = DocumentImportStatus.Imported;
        dbContext.Accounts.AddRange(accounts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkImportFailedAsync(
        Guid documentId,
        string error,
        CancellationToken cancellationToken)
    {
        await dbContext.Documents
            .Where(document => document.Id == documentId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(document => document.ImportStatus, DocumentImportStatus.Failed)
                .SetProperty(document => document.ImportError, error.Length <= 2000 ? error : error[..2000]),
                cancellationToken);
    }

    public Task<Document?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(document => document.Id == id, cancellationToken);

    public Task<int> CountAccountsAsync(Guid documentId, CancellationToken cancellationToken) =>
        dbContext.Accounts.CountAsync(
            account => account.DocumentId == documentId,
            cancellationToken);

    public async Task<IReadOnlyList<Account>> GetAccountsAsync(
        Guid documentId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.DocumentId == documentId)
            .Include(account => account.Classifications.OrderByDescending(result => result.ReceivedAt))
            .OrderBy(account => account.SourceLocation)
            .ThenBy(account => account.SourceRowNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<Account?> GetAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        dbContext.Accounts
            .AsNoTracking()
            .Include(account => account.Document)
            .Include(account => account.Classifications.OrderByDescending(result => result.ReceivedAt))
            .SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken);
}

using FrClassifier.DTOs;
using FrClassifier.Repositories;

namespace FrClassifier.Services;

public sealed class ClassificationDispatchService(
    IAccountRepository accounts,
    IClassificationMessagePublisher publisher,
    ILogger<ClassificationDispatchService> logger)
{
    public async Task<ClassificationDispatchResponse> DispatchPendingAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");
        }

        var pending = await accounts.GetPendingAsync(limit, cancellationToken);
        var queuedCount = 0;
        var failedCount = 0;

        foreach (var account in pending)
        {
            var requestId = Guid.NewGuid();
            if (!await accounts.TryQueueAsync(
                    account.Id,
                    requestId,
                    DateTimeOffset.UtcNow,
                    cancellationToken))
            {
                continue;
            }

            var message = new ClassificationRequestMessage(
                requestId,
                account.Id,
                account.DocumentId,
                account.EntityCode,
                account.FiscalYear,
                account.AccountCode,
                account.AccountName,
                account.Description,
                account.PostingDate,
                account.DocumentDate,
                account.Amount,
                account.CurrencyCode,
                account.ReportingAmount,
                account.ReportingCurrencyCode,
                account.SourceReference,
                account.DimensionsJson);

            try
            {
                await publisher.PublishAsync(message, cancellationToken);
                queuedCount++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await accounts.ResetPendingAsync(account.Id, requestId, CancellationToken.None);
                throw;
            }
            catch (Exception exception)
            {
                await accounts.ResetPendingAsync(account.Id, requestId, CancellationToken.None);
                logger.LogError(exception, "Failed to publish classification request {RequestId}.", requestId);
                failedCount++;
            }
        }

        return new ClassificationDispatchResponse(queuedCount, failedCount);
    }
}
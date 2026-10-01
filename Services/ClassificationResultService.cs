using FrClassifier.DTOs;
using FrClassifier.Entities;
using FrClassifier.Repositories;

namespace FrClassifier.Services;

public sealed class ClassificationResultService(IAccountRepository accounts)
{
    public Task ApplyAsync(
        ClassificationResultMessage message,
        string resultJson,
        CancellationToken cancellationToken)
    {
        Category? category = null;
        if (message.Category is not null)
        {
            if (!CategoryContract.TryParse(message.Category, out var parsedCategory))
            {
                throw new InvalidDataException($"Unknown IFRS 18 category '{message.Category}'.");
            }

            category = parsedCategory;
        }
        else if (!message.NeedsReview)
        {
            throw new InvalidDataException("A classification without a category must require manual review.");
        }

        if (message.RequestId == Guid.Empty || message.AccountId == Guid.Empty)
        {
            throw new InvalidDataException("Classification result is missing its request or account identifier.");
        }

        if (string.IsNullOrWhiteSpace(message.ClassifierName))
        {
            throw new InvalidDataException("Classification result is missing the classifier name.");
        }

        var classification = new AccountClassification
        {
            Id = Guid.NewGuid(),
            AccountId = message.AccountId,
            RequestId = message.RequestId,
            Category = category,
            Confidence = message.Confidence,
            Rationale = message.Rationale,
            NeedsReview = message.NeedsReview,
            ClassifierName = message.ClassifierName,
            ModelVersion = message.ModelVersion,
            ResultJson = resultJson
        };

        return accounts.ApplyClassificationAsync(classification, cancellationToken);
    }
}
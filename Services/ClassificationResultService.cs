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
        if (!CategoryContract.TryParse(message.Category, out var category))
        {
            throw new InvalidDataException($"Unknown IFRS 18 category '{message.Category}'.");
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
            ClassifierName = message.ClassifierName,
            ModelVersion = message.ModelVersion,
            ResultJson = resultJson
        };

        return accounts.ApplyClassificationAsync(classification, cancellationToken);
    }
}
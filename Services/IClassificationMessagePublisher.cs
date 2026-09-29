using FrClassifier.DTOs;

namespace FrClassifier.Services;

public interface IClassificationMessagePublisher
{
    Task PublishAsync(ClassificationRequestMessage message, CancellationToken cancellationToken);
}
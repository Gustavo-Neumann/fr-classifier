using System.Text.Json;
using FrClassifier.DTOs;
using RabbitMQ.Client;

namespace FrClassifier.Services;

public static class ClassificationQueues
{
    public const string Request = "financial.classification.requests";
    public const string Result = "financial.classification.results";
    public const string ResultDeadLetterExchange = "financial.classification.dead";
    public const string ResultDeadLetterQueue = "financial.classification.results.dead";
}

public sealed class RabbitMqClassificationMessagePublisher :
    IClassificationMessagePublisher,
    IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Uri _connectionUri;
    private readonly string _queueName;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqClassificationMessagePublisher(IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMQ:Uri"]
            ?? "amqp://guest:guest@localhost:5672/";
        _connectionUri = new Uri(connectionString);
        _queueName = configuration["RabbitMQ:RequestQueue"] ?? ClassificationQueues.Request;
    }

    public async Task PublishAsync(
        ClassificationRequestMessage message,
        CancellationToken cancellationToken)
    {
        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureConnectedAsync(cancellationToken);
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = message.RequestId.ToString()
            };
            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _publishLock.Dispose();
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        var factory = new ConnectionFactory
        {
            Uri = _connectionUri,
            ClientProvidedName = "fr-classifier:request-publisher"
        };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        _channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken);
        await _channel.QueueDeclareAsync(
            _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}
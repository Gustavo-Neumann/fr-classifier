using System.Text;
using System.Text.Json;
using FrClassifier.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FrClassifier.Services;

public sealed class RabbitMqClassificationResultConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqClassificationResultConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionUri = new Uri(configuration["RabbitMQ:Uri"] ?? "amqp://guest:guest@localhost:5672/");
        var queueName = configuration["RabbitMQ:ResultQueue"] ?? ClassificationQueues.Result;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = connectionUri,
                    ClientProvidedName = "fr-classifier:result-consumer"
                };
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(
                    options: null,
                    cancellationToken: stoppingToken);

                await DeclareResultQueuesAsync(channel, queueName, stoppingToken);
                await channel.BasicQosAsync(0, 1, global: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) =>
                {
                    var body = delivery.Body.ToArray();
                    try
                    {
                        var result = JsonSerializer.Deserialize<ClassificationResultMessage>(body, JsonOptions)
                            ?? throw new InvalidDataException("Classification result payload is empty.");
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var resultService = scope.ServiceProvider.GetRequiredService<ClassificationResultService>();
                        await resultService.ApplyAsync(result, Encoding.UTF8.GetString(body), stoppingToken);
                        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
                    }
                    catch (Exception exception) when (IsInvalidMessage(exception))
                    {
                        logger.LogWarning(exception, "Rejecting invalid classification result message.");
                        await channel.BasicNackAsync(
                            delivery.DeliveryTag,
                            multiple: false,
                            requeue: false,
                            cancellationToken: stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Failed to persist a classification result; requeuing it.");
                        await channel.BasicNackAsync(
                            delivery.DeliveryTag,
                            multiple: false,
                            requeue: true,
                            cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);
                logger.LogInformation("Consuming classification results from queue {QueueName}.", queueName);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "RabbitMQ result consumer unavailable; retrying in five seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task DeclareResultQueuesAsync(
        IChannel channel,
        string queueName,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            ClassificationQueues.ResultDeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            ClassificationQueues.ResultDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(
            ClassificationQueues.ResultDeadLetterQueue,
            ClassificationQueues.ResultDeadLetterExchange,
            ClassificationQueues.ResultDeadLetterQueue,
            cancellationToken: cancellationToken);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = ClassificationQueues.ResultDeadLetterExchange,
            ["x-dead-letter-routing-key"] = ClassificationQueues.ResultDeadLetterQueue
        };
        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }

    private static bool IsInvalidMessage(Exception exception) =>
        exception is JsonException
            or InvalidDataException
            or InvalidOperationException
            or KeyNotFoundException
            or ArgumentOutOfRangeException;
}
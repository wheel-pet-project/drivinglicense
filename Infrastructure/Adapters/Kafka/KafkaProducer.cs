using Application.Ports.Kafka;
using Domain.DrivingLicenceAggregate.DomainEvents;
using From.DrivingLicenseKafkaEvents;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducerProvider topicProducerProvider,
    IOptions<KafkaTopicsConfiguration> configuration) : IMessageBus
{
    private readonly KafkaTopicsConfiguration _configuration = configuration.Value;
    
    public async Task Publish(DrivingLicenseApprovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, DrivingLicenseApproved>(
            new Uri($"topic:{_configuration.DrivingLicenseApprovedTopic}"));
        
        await producer.Produce(domainEvent.EventId.ToString(),
            new DrivingLicenseApproved(domainEvent.EventId, domainEvent.AccountId, [..domainEvent.Categories]),
            Pipe.Execute<KafkaSendContext<string, DrivingLicenseApproved>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }

    public async Task Publish(DrivingLicenseExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, DrivingLicenseExpired>(
            new Uri($"topic:{_configuration.DrivingLicenseExpiredTopic}"));
        
        await producer.Produce(domainEvent.EventId.ToString(),
            new DrivingLicenseExpired(domainEvent.EventId, domainEvent.AccountId),
            Pipe.Execute<KafkaSendContext<string, DrivingLicenseExpired>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}
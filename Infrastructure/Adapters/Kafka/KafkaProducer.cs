using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using Core.Ports.Kafka;
using From.DrivingLicenseKafkaEvents;
using MassTransit;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducer<string, DrivingLicenseApproved> drivingLicenseApprovedProducer,
    ITopicProducer<string, DrivingLicenseExpired> drivingLicenseExpiredProducer) : IMessageBus
{
    public async Task Publish(DrivingLicenseApprovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await drivingLicenseApprovedProducer.Produce(domainEvent.EventId.ToString(),
            new DrivingLicenseApproved(domainEvent.EventId, domainEvent.AccountId, [..domainEvent.Categories]),
            Pipe.Execute<KafkaSendContext<string, DrivingLicenseApproved>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }

    public async Task Publish(DrivingLicenseExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await drivingLicenseExpiredProducer.Produce(domainEvent.EventId.ToString(),
            new DrivingLicenseExpired(domainEvent.EventId, domainEvent.AccountId),
            Pipe.Execute<KafkaSendContext<string, DrivingLicenseExpired>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}
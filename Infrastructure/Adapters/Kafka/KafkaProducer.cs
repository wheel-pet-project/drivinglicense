using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using Core.Ports.Kafka;
using From.DrivingLicenseKafkaEvents;
using MassTransit;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducer<string, DrivingLicenseApproved> sendConfirmationEmailProducer,
    ITopicProducer<string, DrivingLicenseExpired> passwordRecoverTokenCreatedProducer) : IMessageBus
{
    public async Task Publish(DrivingLicenseApprovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await passwordRecoverTokenCreatedProducer.Produce(key: domainEvent.EventId.ToString(),
            new DrivingLicenseApproved(domainEvent.EventId,
            Pipe.Execute<KafkaSendContext<string, DrivingLicenseApproved>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}
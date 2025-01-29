using Core.Domain.DrivingLicenceAggregate.DomainEvents;

namespace Core.Ports.Kafka;

public interface IMessageBus
{
    Task Publish(DrivingLicenseApprovedDomainEvent domainEvent, CancellationToken cancellationToken);

    Task Publish(DrivingLicenseExpiredDomainEvent domainEvent, CancellationToken cancellationToken);
}
using Domain.DrivingLicenceAggregate.DomainEvents;

namespace Application.Ports.Kafka;

public interface IMessageBus
{
    Task Publish(DrivingLicenseApprovedDomainEvent domainEvent, CancellationToken cancellationToken);

    Task Publish(DrivingLicenseExpiredDomainEvent domainEvent, CancellationToken cancellationToken);
}
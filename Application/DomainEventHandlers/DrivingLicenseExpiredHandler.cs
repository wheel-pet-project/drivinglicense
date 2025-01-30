using Application.Ports.Kafka;
using Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class DrivingLicenseExpiredHandler(IMessageBus messageBus)
    : INotificationHandler<DrivingLicenseExpiredDomainEvent>
{
    public async Task Handle(DrivingLicenseExpiredDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}
using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using Core.Ports.Kafka;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class DrivingLicenseExpiredHandler(IMessageBus messageBus) 
    : INotificationHandler<DrivingLicenseExpiredDomainEvent>
{
    public async Task Handle(DrivingLicenseExpiredDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}
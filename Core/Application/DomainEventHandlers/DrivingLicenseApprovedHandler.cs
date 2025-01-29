using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using Core.Ports.Kafka;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class DrivingLicenseApprovedHandler(IMessageBus messageBus) 
    : INotificationHandler<DrivingLicenseApprovedDomainEvent>
{
    public async Task Handle(DrivingLicenseApprovedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}
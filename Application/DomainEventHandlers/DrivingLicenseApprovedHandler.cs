using Application.Ports.Kafka;
using Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class DrivingLicenseApprovedHandler(IMessageBus messageBus)
    : INotificationHandler<DrivingLicenseApprovedDomainEvent>
{
    public async Task Handle(DrivingLicenseApprovedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}
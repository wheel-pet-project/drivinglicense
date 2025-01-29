using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class DrivingLicenseExpiredHandler : INotificationHandler<DrivingLicenseExpiredDomainEvent>
{
    public Task Handle(DrivingLicenseExpiredDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
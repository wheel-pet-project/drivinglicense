using Core.Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class DrivingLicenseApprovedHandler : INotificationHandler<DrivingLicenseApprovedDomainEvent>
{
    public Task Handle(DrivingLicenseApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
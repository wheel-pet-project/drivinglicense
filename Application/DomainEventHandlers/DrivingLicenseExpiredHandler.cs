using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using MediatR;

namespace Application.DomainEventHandlers;

public class DrivingLicenseExpiredHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IMessageBus messageBus)
    : INotificationHandler<DrivingLicenseExpiredDomainEvent>
{
    public async Task Handle(DrivingLicenseExpiredDomainEvent @event, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(@event.DrivingLicenseId);
        if (license is null)
            throw new DataConsistencyViolationException(
                $"{nameof(DrivingLicenseExpiredDomainEvent)} created for not exist license");

        license.Expire(timeProvider);

        drivingLicenseRepository.Update(license);

        var commitResult = await unitOfWork.Commit();
        if (commitResult.IsFailed) throw new TaskCanceledException("Could not commit updates");

        await messageBus.Publish(@event, cancellationToken);
    }
}
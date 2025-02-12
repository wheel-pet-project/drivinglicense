using Application.Ports.Postgres;
using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using MediatR;

namespace Application.DomainEventHandlers;

public class PhotoAddedHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<PhotosAddedDomainEvent>
{
    public async Task Handle(PhotosAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(domainEvent.DrivingLicenseId);
        if (license is null)
            throw new DataConsistencyViolationException(
                $"{nameof(PhotosAddedDomainEvent)} contains {nameof(domainEvent.DrivingLicenseId)} for not existing license");

        license.MarkAsPendingProcessing();

        drivingLicenseRepository.Update(license);
        await unitOfWork.Commit();
    }
}
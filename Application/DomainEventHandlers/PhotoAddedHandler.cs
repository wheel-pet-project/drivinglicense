using Application.Ports.Postgres;
using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using MediatR;

namespace Application.DomainEventHandlers;

public class PhotoAddedHandler(
    IDrivingLicenseRepository drivingLicenseRepository, 
    IUnitOfWork unitOfWork) 
    : INotificationHandler<PhotoAddedDomainEvent>
{
    public async Task Handle(PhotoAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(domainEvent.DrivingLicenseId);
        if (license is null)
            throw new DataConsistencyViolationException(
                $"{nameof(PhotoAddedDomainEvent)} contains {nameof(domainEvent.DrivingLicenseId)} for not existing license");
        
        license.MarkAsPendingProcessing();
        
        drivingLicenseRepository.Update(license);
        await unitOfWork.Commit();
    }
}
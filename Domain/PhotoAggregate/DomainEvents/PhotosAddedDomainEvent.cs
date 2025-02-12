using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.PhotoAggregate.DomainEvents;

public record PhotosAddedDomainEvent : DomainEvent
{
    public PhotosAddedDomainEvent(Guid drivingLicenseId)
    {
        if (drivingLicenseId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(drivingLicenseId)} cannot be empty");

        DrivingLicenseId = drivingLicenseId;
    }

    public Guid DrivingLicenseId { get; private set; }
}
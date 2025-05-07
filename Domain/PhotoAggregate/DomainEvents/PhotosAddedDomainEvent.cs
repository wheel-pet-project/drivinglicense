using Domain.SharedKernel;

namespace Domain.PhotoAggregate.DomainEvents;

public record PhotosAddedDomainEvent : DomainEvent
{
    public PhotosAddedDomainEvent(Guid drivingLicenseId)
    {
        if (drivingLicenseId == Guid.Empty) throw new ArgumentException($"{nameof(drivingLicenseId)} cannot be empty");

        DrivingLicenseId = drivingLicenseId;
    }

    public Guid DrivingLicenseId { get; }
}
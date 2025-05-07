using Domain.SharedKernel;

namespace Domain.DrivingLicenceAggregate.DomainEvents;

public record DrivingLicenseExpiredDomainEvent : DomainEvent
{
    public DrivingLicenseExpiredDomainEvent(Guid drivingLicenseId, Guid accountId)
    {
        if (drivingLicenseId == Guid.Empty) throw new ArgumentException($"{nameof(drivingLicenseId)} cannot be empty");
        if (accountId == Guid.Empty) throw new ArgumentException($"{nameof(accountId)} cannot be empty");

        DrivingLicenseId = drivingLicenseId;
        AccountId = accountId;
    }

    public Guid DrivingLicenseId { get; }
    public Guid AccountId { get; }
}
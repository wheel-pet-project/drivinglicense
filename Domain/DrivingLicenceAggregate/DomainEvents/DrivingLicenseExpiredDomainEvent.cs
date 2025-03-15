using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.DrivingLicenceAggregate.DomainEvents;

public record DrivingLicenseExpiredDomainEvent : DomainEvent
{
    public DrivingLicenseExpiredDomainEvent(Guid drivingLicenseId, Guid accountId)
    {
        if (drivingLicenseId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(drivingLicenseId)} cannot be empty");
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        
        DrivingLicenseId = drivingLicenseId;
        AccountId = accountId;
    }

    public Guid DrivingLicenseId { get; }
    public Guid AccountId { get; }
}
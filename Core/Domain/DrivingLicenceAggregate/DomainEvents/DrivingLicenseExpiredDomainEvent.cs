using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.DrivingLicenceAggregate.DomainEvents;

public record DrivingLicenseExpiredDomainEvent : DomainEvent
{
    public DrivingLicenseExpiredDomainEvent(Guid accountId)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        
        AccountId = accountId;
    }
    
    public Guid AccountId { get; private set; }
}
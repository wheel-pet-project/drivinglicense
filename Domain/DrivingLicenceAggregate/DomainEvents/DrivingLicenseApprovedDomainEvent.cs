using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.DrivingLicenceAggregate.DomainEvents;

public record DrivingLicenseApprovedDomainEvent : DomainEvent
{
    public DrivingLicenseApprovedDomainEvent(Guid accountId, List<char> categories)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (categories is null || categories.Count == 0)
            throw new ValueIsRequiredException($"{nameof(categories)} cannot be null");

        AccountId = accountId;
        Categories = categories;
    }

    public Guid AccountId { get; }

    public IReadOnlyList<char> Categories { get; }
}
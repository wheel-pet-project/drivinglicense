using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.DrivingLicenceAggregate.DomainEvents;

public record CategoriesRegisteredForDrivingLicenseDomainEvent : DomainEvent
{
    public CategoriesRegisteredForDrivingLicenseDomainEvent(Guid accountId, List<Category> categories)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (categories is null || !categories.Any())
            throw new ValueIsRequiredException($"{nameof(categories)} cannot be null or empty");
        
        AccountId = accountId;
        Categories = categories.Select(x => x.CategorySymbol).ToArray();
    }
    
    public Guid AccountId { get; private set; }
    
    public char[] Categories { get; private set; } 
}
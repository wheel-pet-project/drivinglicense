using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.DrivingLicenceAggregate.DomainEvents;

public record CategoriesRegisteredForDrivingLicenseDomainEvent : DomainEvent
{
    public CategoriesRegisteredForDrivingLicenseDomainEvent(Guid accountId, CategoryList categoryList)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (categoryList is null || !categoryList.Categories.Any())
            throw new ValueIsRequiredException($"{nameof(categoryList)} cannot be null or empty");
        
        AccountId = accountId;
        Categories = categoryList.Categories;
    }
    
    public Guid AccountId { get; private set; }
    
    public IReadOnlyList<char> Categories { get; private set; } 
}
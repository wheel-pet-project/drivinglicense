using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using CSharpFunctionalExtensions;

namespace Core.Domain.DrivingLicenceAggregate;

public class Category : Entity<Guid>
{
    public static readonly Category BCategory = new('B');
    
    
    private Category(){}

    private Category(char categorySymbol)
    {
        CategorySymbol = categorySymbol;
    }
    
    public char CategorySymbol { get; private set; }


    public static IEnumerable<Category> All()
    {
        return [BCategory];
    }

    public Category FromCategorySymbol(char categorySymbol)
    {
        var category = All().SingleOrDefault(_ => Equals(categorySymbol));
        if (category == null) throw new ValueOutOfRangeException($"{nameof(categorySymbol)} unsupported or null");
        
        return category;
    }
}
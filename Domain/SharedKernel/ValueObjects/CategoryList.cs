using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.SharedKernel.ValueObjects;

public class CategoryList : ValueObject
{
    public static readonly char BCategory = 'B';
    
    private CategoryList(){}

    private CategoryList(List<char> categories) : this()
    {
        _categories = categories;
    }
    
    
    private List<char> _categories = null!;
    public IReadOnlyList<char> Categories => _categories.AsReadOnly();

    public static IEnumerable<char> GetSupportedCategories()
    {
        return [BCategory];
    }

    public static CategoryList Create(List<char> categories)
    {
        if (categories.Count == 0) throw new ValueIsRequiredException($"{nameof(categories)} cannot be empty");
        if (!categories.All(x => GetSupportedCategories().Contains(x)))
            throw new ValueOutOfRangeException($"{nameof(categories)} contains not a supported category");
        
        return new CategoryList(categories);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var category in _categories)
            yield return category;
    }
}
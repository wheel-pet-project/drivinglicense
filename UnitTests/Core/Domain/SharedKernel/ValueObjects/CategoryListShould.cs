using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(CategoryList))]
public class CategoryListShould
{
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange
        var category = 'B';

        // Act
        var actual = CategoryList.Create([CategoryList.BCategory]);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(category, actual.Categories[0]);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfCategoryIsUnsupported()
    {
        // Arrange
        var unsupportedCategory = 'Y';

        // Act
        void Act() => CategoryList.Create([unsupportedCategory]);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualCategories()
    {
        // Arrange
        var category1 = CategoryList.Create([CategoryList.BCategory]);
        var category2 = CategoryList.Create([CategoryList.BCategory]);

        // Act
        var actual = category1 == category2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NonEqualOperatorReturnFalseForEqualCategories()
    {
        // Arrange
        var category1 = CategoryList.Create([CategoryList.BCategory]);
        var category2 = CategoryList.Create([CategoryList.BCategory]);

        // Act
        var actual = category1 != category2;

        // Assert
        Assert.False(actual);
    }
}
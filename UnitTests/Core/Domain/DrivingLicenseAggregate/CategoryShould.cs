using System.Reflection;
using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

public class CategoryShould
{
    [Fact]
    public void CanCreateInstanceWithCorrectValues()
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
    public void CanThrowValueOutOfRangeExceptionIfCategoryIsUnsupported()
    {
        // Arrange
        var unsupportedCategory = 'Y';

        // Act
        void Act() => CategoryList.Create([unsupportedCategory]);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void CanEqualOperatorReturnTrueForEqualCategories()
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
    public void CanNonEqualOperatorReturnFalseForEqualCategories()
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
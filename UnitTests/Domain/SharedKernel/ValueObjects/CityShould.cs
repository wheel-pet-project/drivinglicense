using Domain.SharedKernel.Exceptions.PublicExceptions;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(City))]
public class CityShould
{
    [Fact]
    public void ReturnNewInstanceWithCorrectValue()
    {
        // Arrange
        var cityName = "Москва";

        // Act
        var city = City.Create(cityName);

        // Assert
        Assert.NotNull(city);
        Assert.Equal(cityName, city.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfNameIsInvalid(string invalidName)
    {
        // Arrange

        // Act
        void Act()
        {
            City.Create(invalidName);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void TrimName()
    {
        // Arrange
        var cityName = "  Москва  ";

        // Act
        var city = City.Create(cityName);

        // Assert
        Assert.NotNull(city);
        Assert.Equal("Москва", city.Name);
    }

    [Fact]
    public void UpperFirstLetter()
    {
        // Arrange
        var cityName = " москва ";

        // Act
        var city = City.Create(cityName);

        // Assert
        Assert.NotNull(city);
        Assert.Equal("Москва", city.Name);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualCities()
    {
        // Arrange
        var city1 = City.Create("Москва");
        var city2 = City.Create("Москва");

        // Act
        var actual = city1 == city2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EqualOperatorReturnFalseForDifferentCities()
    {
        // Arrange
        var city1 = City.Create("Paris");
        var city2 = City.Create("Москва");

        // Act
        var actual = city1 == city2;

        // Assert
        Assert.False(actual);
    }
}
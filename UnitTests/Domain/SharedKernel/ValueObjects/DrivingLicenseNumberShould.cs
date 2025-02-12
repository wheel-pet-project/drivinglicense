using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(DrivingLicenseNumber))]
public class DrivingLicenseNumberShould
{
    [Fact]
    public void CreateInstanceWithCorrectNumber()
    {
        // Arrange
        var input = "1255 102030";

        // Act
        var actual = DrivingLicenseNumber.Create(input);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("1255102030", actual.Value);
    }

    [Fact]
    public void TrimInputNumber()
    {
        // Arrange
        var input = "  1255 102030  ";

        // Act
        var actual = DrivingLicenseNumber.Create(input);

        // Assert
        Assert.Equal("1255102030", actual.Value);
    }

    [Fact]
    public void ReplaceWhiteSpaces()
    {
        // Arrange
        var input = "  1255 10 2 030  ";

        // Act
        var actual = DrivingLicenseNumber.Create(input);

        // Assert
        Assert.Equal("1255102030", actual.Value);
    }

    [Theory]
    [InlineData("12441214121")] // 11 symbols
    [InlineData("999999999")] // 9 symbols
    public void ThrowValueOutOfRangeExceptionIfNumberLengthIsNot10(string invalidCode)
    {
        // Arrange

        // Act
        void Act()
        {
            DrivingLicenseNumber.Create(invalidCode);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Theory]
    [InlineData("1244 12_412")] //
    [InlineData("  a 244 121412")] // (length after trim == 10)
    [InlineData("1244 1#1412")] //
    public void ThrowValueIsInvalidExceptionIfNumberContainAnySymbolOtherThanDigit(string invalidCode)
    {
        // Arrange

        // Act
        void Act()
        {
            DrivingLicenseNumber.Create(invalidCode);
        }

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualNumbers()
    {
        // Arrange
        var number1 = DrivingLicenseNumber.Create("1255 102030");
        var number2 = DrivingLicenseNumber.Create("1255 102030");

        // Act
        var actual = number1 == number2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EqualOperatorReturnFalseForDifferentNumbers()
    {
        // Arrange
        var number1 = DrivingLicenseNumber.Create("1255 102030");
        var number2 = DrivingLicenseNumber.Create("3242 123561");

        // Act
        var actual = number1 == number2;

        // Assert
        Assert.False(actual);
    }
}
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(DrivingLicenseNumber))]
public class DrivingLicenseNumberShould
{
    [Fact]
    public void CanCreateInstanceWithCorrectNumber()
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
    public void CanTrimInputNumber()
    {
        // Arrange
        var input = "  1255 102030  ";

        // Act
        var actual = DrivingLicenseNumber.Create(input);
        
        // Assert
        Assert.Equal("1255102030", actual.Value);
    }

    [Fact]
    public void CanReplaceWhiteSpaces()
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
    public void CanThrowValueOutOfRangeExceptionIfNumberLengthIsNot10(string invalidCode)
    {
        // Arrange

        // Act
        void Act() => DrivingLicenseNumber.Create(invalidCode);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Theory]
    [InlineData("1244 12_412")]    //
    [InlineData("  a 244 121412")] // (length after trim == 10)
    [InlineData("1244 1#1412")]    //
    public void CanThrowValueIsInvalidExceptionIfNumberContainAnySymbolOtherThanDigit(string invalidCode)
    {
        // Arrange

        // Act
        void Act() => DrivingLicenseNumber.Create(invalidCode);
        
        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }
    
    [Fact]
    public void CanEqualOperatorReturnTrueForEqualNumbers()
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
    public void CanEqualOperatorReturnFalseForDifferentNumbers()
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
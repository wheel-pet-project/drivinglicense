using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(Name))]
public class NameShould
{
    [Fact]
    public void CanCreateNameWithCorrectValues()
    {
        // Arrange
        var firstName = "Иван";
        var lastName = "Иванов";
        var patronymic = "Иванович";

        // Act
        var actual = Name.Create(firstName, lastName, patronymic);

        // Assert
        Assert.Equivalent(firstName, actual.FirstName);
        Assert.Equivalent(lastName, actual.LastName);
        Assert.Equivalent(patronymic, actual.Patronymic);
    }

    [Fact]
    public void CanEqualOperatorReturnTrueForEqualNames()
    {
        // Arrange
        var firstName = "Иван";
        var lastName = "Иванов";
        var patronymic = "Иванович";
        var name1 = Name.Create(firstName, lastName, patronymic);
        var name2 = Name.Create(firstName, lastName, patronymic);

        // Act
        var actual = name1 == name2;
        
        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void CanEqualOperatorReturnFalseForDifferentNames()
    {
        // Arrange
        var firstName = "Иван";
        var lastName = "Иванов";
        var patronymic = "Иванович";
        var name1 = Name.Create(firstName, lastName, patronymic);
        var name2 = Name.Create(firstName, lastName); // without patronymic

        // Act
        var actual = name1 == name2;
        
        // Assert
        Assert.False(actual);
    }

    [Theory]
    [InlineData("", "Иванов", "Иванович")]   //
    [InlineData("  ", "Иванов", "Иванович")] // invalid first name
    [InlineData(null, "Иванов", "Иванович")] // 
    
    [InlineData("Иван", "", "Иванович")]     //
    [InlineData("Иван", "  ", "Иванович")]   // invalid last name
    [InlineData("Иван", null, "Иванович")]   // 
    
    [InlineData("Иван", "Иванов", "")]       // empty patronymic
    [InlineData("Иван", "Иванов", "  ")]     // 
    public void CanThrowValueIsRequiredExceptionIfSomeNamePartIsInvalid(string? firstName, string? lastName, 
        string? patronymic)
    {
        // Arrange

        // Act
        void Act() => Name.Create(firstName, lastName, patronymic);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
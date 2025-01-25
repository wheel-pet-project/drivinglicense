using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(CodeOfIssue))]
public class CodeOfIssueShould
{
    [Fact]
    public void CreateInstanceWithCorrectCode()
    {
        // Arrange
        var input = "1255";

        // Act
        var actual = CodeOfIssue.Create(input);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(input, actual.Value);
    }

    [Fact]
    public void TrimInputCode()
    {
        // Arrange
        var input = " 1255  ";

        // Act
        var actual = CodeOfIssue.Create(input);


        // Assert
        Assert.Equal("1255", actual.Value);
    }

    [Theory]
    [InlineData("55555")]
    [InlineData("333")]
    public void ThrowValueOutOfRangeExceptionIfCodeLengthIsNot4(string invalidCode)
    {
        // Arrange

        // Act
        void Act() => CodeOfIssue.Create(invalidCode);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Theory]
    [InlineData("12_2")]
    [InlineData("#112")]
    [InlineData("12r1")]
    public void ThrowValueIsInvalidExceptionIfCodeContainAnySymbolOtherThanDigit(string invalidCode)
    {
        // Arrange

        // Act
        void Act() => CodeOfIssue.Create(invalidCode);
        
        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }
    
    [Fact]
    public void EqualOperatorReturnTrueForEqualCodes()
    {
        // Arrange
        var input = "1255";
        var codeOfIssue1 = CodeOfIssue.Create(input);
        var codeOfIssue2 = CodeOfIssue.Create(input);

        // Act
        var actual = codeOfIssue1 == codeOfIssue2;
        
        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EqualOperatorReturnFalseForDifferentCodes()
    {
        // Arrange
        var codeOfIssue1 = CodeOfIssue.Create("1111");
        var codeOfIssue2 = CodeOfIssue.Create("7777");

        // Act
        var actual = codeOfIssue1 == codeOfIssue2;
        
        // Assert
        Assert.False(actual);
    }
}
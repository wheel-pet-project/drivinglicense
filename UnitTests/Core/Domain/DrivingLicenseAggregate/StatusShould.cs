using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

public class StatusShould
{
    [Fact]
    public void CanReturnRightStatusFromName()
    {
        // Arrange
        var unprocessedName = Status.Unprocessed.Name;

        // Act
        var actual = Status.FromName(unprocessedName);

        // Assert
        Assert.Equal(Status.Unprocessed, actual);
    }

    [Fact]
    public void CanThrowValueOutOfRangeExceptionIfStatusNameIsUnknown()
    {
        // Arrange
        var invalidName = "unsupportedName";

        // Act
        void Act() => Status.FromName(invalidName);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
    
    [Fact]
    public void CanReturnRightStatusFromId()
    {
        // Arrange
        var unprocessedId = Status.Unprocessed.Id;

        // Act
        var actual = Status.FromId(unprocessedId);

        // Assert
        Assert.Equal(Status.Unprocessed, actual);
    }

    [Fact]
    public void CanThrowValueOutOfRangeExceptionIfStatusIdIsUnknown()
    {
        // Arrange
        var invalidId = 111;

        // Act
        void Act() => Status.FromId(invalidId);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void CanEqualOperatorReturnTrueForEqualStatuses()
    {
        // Arrange
        var status1 = Status.Unprocessed;
        var status2 = Status.Unprocessed;

        // Act
        var actual = status1 == status2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void CanEqualOperatorReturnFalseForDifferentStatuses()
    {
        // Arrange
        var status1 = Status.Unprocessed;
        var status2 = Status.Approved;

        // Act
        var actual = status1 == status2;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void CanNonEqualOperatorReturnFalseForEqualStatuses()
    {
        // Arrange
        var status1 = Status.Unprocessed;
        var status2 = Status.Unprocessed;

        // Act
        var actual = status1 != status2;

        // Assert
        Assert.False(actual);
    }
}
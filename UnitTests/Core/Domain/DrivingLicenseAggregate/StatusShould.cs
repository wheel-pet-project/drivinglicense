using System.Reflection;
using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

public class StatusShould
{
    [Fact]
    public void ReturnRightStatusFromName()
    {
        // Arrange
        var unprocessedName = Status.Unprocessed.Name;

        // Act
        var actual = Status.FromName(unprocessedName);

        // Assert
        Assert.Equal(Status.Unprocessed, actual);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfStatusNameIsUnknown()
    {
        // Arrange
        var invalidName = "unsupportedName";

        // Act
        void Act() => Status.FromName(invalidName);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
    
    [Fact]
    public void ReturnRightStatusFromId()
    {
        // Arrange
        var unprocessedId = Status.Unprocessed.Id;

        // Act
        var actual = Status.FromId(unprocessedId);

        // Assert
        Assert.Equal(Status.Unprocessed, actual);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfStatusIdIsUnknown()
    {
        // Arrange
        var invalidId = 111;

        // Act
        void Act() => Status.FromId(invalidId);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualStatuses()
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
    public void EqualOperatorReturnFalseForDifferentStatuses()
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
    public void NonEqualOperatorReturnFalseForEqualStatuses()
    {
        // Arrange
        var status1 = Status.Unprocessed;
        var status2 = Status.Unprocessed;

        // Act
        var actual = status1 != status2;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ReturnTrueForCorrectPotentialStatuses()
    {
        // Arrange
        var unprocessed = Status.Unprocessed;
        var approved = Status.Approved;

        // Act
        var unprocessedToApproved = unprocessed.CanBeChangedToThisStatus(Status.Approved);
        var unprocessedToDeclined = unprocessed.CanBeChangedToThisStatus(Status.Declined);
        var approvedToExpired = approved.CanBeChangedToThisStatus(Status.Expired);

        // Assert
        Assert.True(unprocessedToApproved);
        Assert.True(unprocessedToDeclined);
        Assert.True(approvedToExpired);
    }

    [Fact]
    public void ReturnFalseForIncorrectPotentialStatuses()
    {
        // Arrange
        var unprocessed = Status.Unprocessed;
        var approved = Status.Approved;
        var declined = Status.Declined;
        var expired = Status.Expired;

        // Act
        var unprocessedToExpired = unprocessed.CanBeChangedToThisStatus(Status.Expired);
        
        var approvedToDeclined = approved.CanBeChangedToThisStatus(Status.Declined);
        var approvedToUnprocessed = approved.CanBeChangedToThisStatus(Status.Unprocessed);
        
        var declinedToExpired = declined.CanBeChangedToThisStatus(Status.Declined);
        var declinedToApproved = declined.CanBeChangedToThisStatus(Status.Approved);
        var declinedToUnprocessed = declined.CanBeChangedToThisStatus(Status.Unprocessed);
        
        var expiredToDeclined = expired.CanBeChangedToThisStatus(Status.Declined);
        var expiredToUnprocessed = expired.CanBeChangedToThisStatus(Status.Unprocessed);
        var expiredToApproved = expired.CanBeChangedToThisStatus(Status.Approved);
        
        // Assert
        Assert.False(unprocessedToExpired);
        
        Assert.False(approvedToDeclined);
        Assert.False(approvedToUnprocessed);
        
        Assert.False(declinedToExpired);
        Assert.False(declinedToApproved);
        Assert.False(declinedToUnprocessed);
        
        Assert.False(expiredToDeclined);
        Assert.False(expiredToUnprocessed);
        Assert.False(expiredToApproved);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfPotentialStatusIsNull()
    {
        // Arrange
        var status = Status.Unprocessed;

        // Act
        void Act() => status.CanBeChangedToThisStatus(null); 

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
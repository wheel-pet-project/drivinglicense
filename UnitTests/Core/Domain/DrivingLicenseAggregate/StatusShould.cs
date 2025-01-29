using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

[TestSubject(typeof(Status))]
public class StatusShould
{
    [Fact]
    public void ReturnRightStatusFromName()
    {
        // Arrange
        var pendingProcessingName = Status.PendingProcessing.Name;

        // Act
        var actual = Status.FromName(pendingProcessingName);

        // Assert
        Assert.Equal(Status.PendingProcessing, actual);
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
        var pendingProcessingId = Status.PendingProcessing.Id;

        // Act
        var actual = Status.FromId(pendingProcessingId);

        // Assert
        Assert.Equal(Status.PendingProcessing, actual);
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
        var status1 = Status.PendingProcessing;
        var status2 = Status.PendingProcessing;

        // Act
        var actual = status1 == status2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EqualOperatorReturnFalseForDifferentStatuses()
    {
        // Arrange
        var status1 = Status.PendingProcessing;
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
        var status1 = Status.PendingProcessing;
        var status2 = Status.PendingProcessing;

        // Act
        var actual = status1 != status2;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ReturnTrueForCorrectPotentialStatuses()
    {
        // Arrange
        var pendingPhotosAdding = Status.PendingPhotosAdding;
        var pendingProcessing = Status.PendingProcessing;
        var approved = Status.Approved;

        // Act
        var pendingPhotosAddingToPendingProcessing =
            pendingPhotosAdding.CanBeChangedToThisStatus(Status.PendingProcessing);
        var pendingProcessingToApproved = pendingProcessing.CanBeChangedToThisStatus(Status.Approved);
        var pendingProcessingToRejected = pendingProcessing.CanBeChangedToThisStatus(Status.Rejected);
        var approvedToExpired = approved.CanBeChangedToThisStatus(Status.Expired);

        // Assert
        Assert.True(pendingPhotosAddingToPendingProcessing);
        Assert.True(pendingProcessingToApproved);
        Assert.True(pendingProcessingToRejected);
        Assert.True(approvedToExpired);
    }

    [Fact]
    public void ReturnFalseForIncorrectPotentialStatuses()
    {
        // Arrange
        var pendingPhotosAdding = Status.PendingPhotosAdding;
        var pendingProcessing = Status.PendingProcessing;
        var approved = Status.Approved;
        var rejected = Status.Rejected;
        var expired = Status.Expired;

        // Act
        var pendingPhotosAddingToApproved = pendingPhotosAdding.CanBeChangedToThisStatus(Status.Approved);
        var pendingPhotosAddingToRejected = pendingPhotosAdding.CanBeChangedToThisStatus(Status.Rejected);
        var pendingPhotosAddingToExpired = pendingPhotosAdding.CanBeChangedToThisStatus(Status.Expired);
        
        var pendingProcessingToExpired = pendingProcessing.CanBeChangedToThisStatus(Status.Expired);
        var pendingProcessingToPendingPhotosAdding =
            pendingProcessing.CanBeChangedToThisStatus(Status.PendingPhotosAdding);
        
        var approvedToRejected = approved.CanBeChangedToThisStatus(Status.Rejected);
        var approvedToUnprocessed = approved.CanBeChangedToThisStatus(Status.PendingProcessing);
        var approvedToPendingPhotosAdding = approved.CanBeChangedToThisStatus(Status.PendingPhotosAdding);
        
        var rejectedToExpired = rejected.CanBeChangedToThisStatus(Status.Rejected);
        var rejectedToApproved = rejected.CanBeChangedToThisStatus(Status.Approved);
        var rejectedToUnprocessed = rejected.CanBeChangedToThisStatus(Status.PendingProcessing);
        var rejectedToPendingPhotosAdding = rejected.CanBeChangedToThisStatus(Status.PendingPhotosAdding);
        
        var expiredToRejected = expired.CanBeChangedToThisStatus(Status.Rejected);
        var expiredToUnprocessed = expired.CanBeChangedToThisStatus(Status.PendingProcessing);
        var expiredToApproved = expired.CanBeChangedToThisStatus(Status.Approved);
        var expiredToPendingPhotosAdding = expired.CanBeChangedToThisStatus(Status.PendingPhotosAdding);
        
        // Assert
        Assert.False(pendingPhotosAddingToApproved);
        Assert.False(pendingPhotosAddingToRejected);
        Assert.False(pendingPhotosAddingToExpired);
        
        Assert.False(pendingProcessingToExpired);
        Assert.False(pendingProcessingToPendingPhotosAdding);
        
        Assert.False(approvedToRejected);
        Assert.False(approvedToUnprocessed);
        Assert.False(approvedToPendingPhotosAdding);
        
        Assert.False(rejectedToExpired);
        Assert.False(rejectedToApproved);
        Assert.False(rejectedToUnprocessed);
        Assert.False(rejectedToPendingPhotosAdding);
        
        Assert.False(expiredToRejected);
        Assert.False(expiredToUnprocessed);
        Assert.False(expiredToApproved);
        Assert.False(expiredToPendingPhotosAdding);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfPotentialStatusIsNull()
    {
        // Arrange
        var status = Status.PendingProcessing;

        // Act
        void Act() => status.CanBeChangedToThisStatus(null); 

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
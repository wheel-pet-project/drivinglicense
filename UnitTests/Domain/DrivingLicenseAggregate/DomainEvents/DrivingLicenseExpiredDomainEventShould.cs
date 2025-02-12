using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.DrivingLicenseAggregate.DomainEvents;

[TestSubject(typeof(DrivingLicenseApprovedDomainEvent))]
public class DrivingLicenseExpiredDomainEventShould
{
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var domainEvent = new DrivingLicenseExpiredDomainEvent(accountId);

        // Assert
        Assert.Equal(accountId, domainEvent.AccountId);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionWhenAccountIdIsEmpty()
    {
        // Arrange
        var accountId = Guid.Empty;

        // Act
        void Act()
        {
            new DrivingLicenseExpiredDomainEvent(accountId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
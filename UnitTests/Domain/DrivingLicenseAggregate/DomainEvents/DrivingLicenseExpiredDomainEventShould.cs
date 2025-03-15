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
        var licenseId = Guid.NewGuid();

        // Act
        var domainEvent = new DrivingLicenseExpiredDomainEvent(licenseId, accountId);

        // Assert
        Assert.Equal(accountId, domainEvent.AccountId);
        Assert.Equal(licenseId, domainEvent.DrivingLicenseId);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionWhenAccountIdIsEmpty()
    {
        // Arrange
        var accountId = Guid.Empty;
        var licenseId = Guid.NewGuid();

        // Act
        void Act()
        {
            new DrivingLicenseExpiredDomainEvent(licenseId, accountId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionWhenLicenseIdIsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var licenseId = Guid.Empty;

        // Act
        void Act()
        {
            new DrivingLicenseExpiredDomainEvent(licenseId, accountId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
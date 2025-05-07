using Domain.DrivingLicenceAggregate.DomainEvents;
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
    public void ThrowArgumentExceptionWhenAccountIdIsEmpty()
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
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionWhenLicenseIdIsEmpty()
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
        Assert.Throws<ArgumentException>(Act);
    }
}
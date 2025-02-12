using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.DrivingLicenseAggregate.DomainEvents;

[TestSubject(typeof(DrivingLicenseApprovedDomainEvent))]
public class DrivingLicenseApprovedDomainEventShould
{
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange
        var categoryList = CategoryList.Create([CategoryList.BCategory]);
        var accountId = Guid.NewGuid();

        // Act
        var domainEvent = new DrivingLicenseApprovedDomainEvent(accountId, [..categoryList.Categories]);

        // Assert
        Assert.Equal(accountId, domainEvent.AccountId);
        Assert.Equal(categoryList.Categories, domainEvent.Categories);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfAccountIdIsEmpty()
    {
        // Arrange
        var categoryList = CategoryList.Create([CategoryList.BCategory]);
        var accountId = Guid.Empty;

        // Act
        void Act()
        {
            new DrivingLicenseApprovedDomainEvent(accountId, [..categoryList.Categories]);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfCategoryListIsNull()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        void Act()
        {
            new DrivingLicenseApprovedDomainEvent(accountId, null);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
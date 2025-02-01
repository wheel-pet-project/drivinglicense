using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.PhotoAggregate.DomainEvents;

[TestSubject(typeof(PhotoAddedDomainEvent))]
public class PhotoAddedDomainEventShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var domainEvent = new PhotoAddedDomainEvent(id);

        // Assert
        Assert.Equal(id, domainEvent.DrivingLicenseId);
    }
    
    [Fact]
    public void ThrowValueIsRequiredExceptionWhenDrivingLicenseIdIsEmpty()
    {
        // Arrange

        // Act
        void Act() => new PhotoAddedDomainEvent(Guid.Empty);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
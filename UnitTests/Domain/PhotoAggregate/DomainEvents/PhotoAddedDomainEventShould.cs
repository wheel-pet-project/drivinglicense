using Domain.PhotoAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.PhotoAggregate.DomainEvents;

[TestSubject(typeof(PhotosAddedDomainEvent))]
public class PhotoAddedDomainEventShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var domainEvent = new PhotosAddedDomainEvent(id);

        // Assert
        Assert.Equal(id, domainEvent.DrivingLicenseId);
    }

    [Fact]
    public void ThrowArgumentExceptionWhenDrivingLicenseIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            new PhotosAddedDomainEvent(Guid.Empty);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}
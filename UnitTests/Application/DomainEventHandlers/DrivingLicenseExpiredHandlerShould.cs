using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Domain.DrivingLicenceAggregate.DomainEvents;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(DrivingLicenseExpiredHandler))]
public class DrivingLicenseExpiredHandlerShould
{
    private readonly DrivingLicenseExpiredDomainEvent _domainEvent = new(Guid.NewGuid());
    [Fact]
    public async Task PublishCallMessageBus()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var handler = new DrivingLicenseExpiredHandler(messageBusMock.Object);

        // Act
        await handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        messageBusMock.Verify(x => x.Publish(_domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }
}
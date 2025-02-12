using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(DrivingLicenseApprovedHandler))]
public class DrivingLicenseApprovedHandlerShould
{
    private readonly DrivingLicenseApprovedDomainEvent _domainEvent = new(Guid.NewGuid(), [CategoryList.BCategory]);

    [Fact]
    public async Task PublishCallMessageBus()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var handler = new DrivingLicenseApprovedHandler(messageBusMock.Object);

        // Act
        await handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        messageBusMock.Verify(x => x.Publish(_domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }
}
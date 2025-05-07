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
    private readonly DrivingLicenseExpiredDomainEvent _domainEvent = new(Guid.NewGuid(), Guid.NewGuid());
    
    private readonly Mock<IMessageBus> _messageBusMock = new();

    private readonly DrivingLicenseExpiredHandler _handler;

    public DrivingLicenseExpiredHandlerShould()
    {
        _handler = new DrivingLicenseExpiredHandler(_messageBusMock.Object);
    }

    [Fact]
    public async Task CallPublishInMessageBus()
    {
        // Arrange

        // Act
        await _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _messageBusMock.Verify(x =>
            x.Publish(It.IsAny<DrivingLicenseExpiredDomainEvent>(), It.IsAny<CancellationToken>()));
    }
}
using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(DrivingLicenseExpiredHandler))]
public class DrivingLicenseExpiredHandlerShould
{
    private readonly DrivingLicenseExpiredDomainEvent _domainEvent = new(Guid.NewGuid(), Guid.NewGuid());

    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create("1234 567891"),
        Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
        new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
        TimeProvider.System);

    private readonly Mock<IDrivingLicenseRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTimeProvider = new();
    private readonly Mock<IMessageBus> _messageBusMock = new();

    private readonly DrivingLicenseExpiredHandler _handler;

    public DrivingLicenseExpiredHandlerShould()
    {
        _drivingLicense.MarkAsPendingProcessing();
        _drivingLicense.Approve();
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddYears(1).AddDays(1));
        _repositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(_drivingLicense);
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Ok);
        _handler = new DrivingLicenseExpiredHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _fakeTimeProvider,
            _messageBusMock.Object);
    }

    [Fact]
    public async Task CommitUpdates()
    {
        // Arrange

        // Act
        await _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ThrowDataConsistencyViolationExceptionIfVehicleDocumentsNotFound()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as DrivingLicense);

        // Act
        Task Act()
        {
            return _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }

    [Fact]
    public async Task ThrowTaskCanceledExceptionIfCommitFailed()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Fail("error"));

        // Act
        Task Act()
        {
            return _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(Act);
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
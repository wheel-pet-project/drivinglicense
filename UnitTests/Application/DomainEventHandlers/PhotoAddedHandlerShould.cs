using Application.DomainEventHandlers;
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.InternalExceptions;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(PhotoAddedHandler))]
public class PhotoAddedHandlerShould
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create("1234 567891"),
        Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
        new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1),
        TimeProvider.System);

    private readonly PhotosAddedDomainEvent _domainEvent = new(Guid.NewGuid());

    [Fact]
    public async Task MutateLicenseStatus()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureDrivingLicenseRepository(_drivingLicense);
        var handler = handlerBuilder.Build();

        // Act
        await handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(Status.PendingProcessing, _drivingLicense.Status);
    }

    [Fact]
    public async Task UnitOfWorkCommitCall()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureDrivingLicenseRepository(_drivingLicense);
        var handler = handlerBuilder.Build();

        // Act
        await handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        handlerBuilder.VerifyUnitOfWorkCall();
    }

    [Fact]
    public async Task ThrowDataConsistencyViolationExceptionIfGetByIdReturnNulls()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureDrivingLicenseRepository(null!);
        var handler = handlerBuilder.Build();

        // Act
        async Task Act()
        {
            await handler.Handle(_domainEvent, TestContext.Current.CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }

    private class HandlerBuilder
    {
        private readonly Mock<IDrivingLicenseRepository> _drivingLicenseRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public PhotoAddedHandler Build()
        {
            return new PhotoAddedHandler(_drivingLicenseRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        public void ConfigureDrivingLicenseRepository(DrivingLicense getByIdShouldReturn)
        {
            _drivingLicenseRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(getByIdShouldReturn);
        }

        public void VerifyUnitOfWorkCall()
        {
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }
    }
}
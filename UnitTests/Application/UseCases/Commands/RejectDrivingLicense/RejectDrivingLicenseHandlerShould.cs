using Application.Ports.Postgres;
using Application.UseCases.Commands.RejectDrivingLicense;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.UseCases.Commands.RejectDrivingLicense;

[TestSubject(typeof(RejectDrivingLicenseHandler))]
public class RejectDrivingLicenseHandlerShould
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create("1234 567891"),
        Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
        new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1),
        TimeProvider.System);

    private readonly RejectDrivingLicenseCommand _command = new(Guid.NewGuid());

    [Fact]
    public async Task ReturnOk()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        _drivingLicense.MarkAsPendingProcessing();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureDrivingLicenseRepository(_drivingLicense);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task ReturnNotFound()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureDrivingLicenseRepository(null);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
        Assert.True(response.Errors[0].GetType() == typeof(NotFound));
    }

    [Fact]
    public async Task ReturnFailIfCommitFailed()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        _drivingLicense.MarkAsPendingProcessing();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureDrivingLicenseRepository(_drivingLicense);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccess);
    }

    private class HandlerBuilder
    {
        private readonly Mock<IDrivingLicenseRepository> _drivingLicenseRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public RejectDrivingLicenseHandler Build()
        {
            return new RejectDrivingLicenseHandler(_drivingLicenseRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        public void ConfigureDrivingLicenseRepository(DrivingLicense? getByIdShouldReturn)
        {
            _drivingLicenseRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(getByIdShouldReturn);
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn)
        {
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);
        }
    }
}
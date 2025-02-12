using Application.Ports.Postgres;
using Application.UseCases.Commands.UploadDrivingLicense;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.Commands.UploadDrivingLicense;

[TestSubject(typeof(UploadDrivingLicenseHandler))]
public class UploadDrivingLicenseHandlerShould
{
    private readonly UploadDrivingLicenseCommand _command = new(
        Guid.NewGuid(), Guid.NewGuid(), [CategoryList.BCategory], "1234 567891", "Иван", "Иванов", "Иванович", "Москва",
        new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "1234", new DateOnly(2030, 1, 1));

    [Fact]
    public async Task ReturnOk()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
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

        public UploadDrivingLicenseHandler Build()
        {
            return new UploadDrivingLicenseHandler(_drivingLicenseRepositoryMock.Object, TimeProvider.System,
                _unitOfWorkMock.Object);
        }
    }
}
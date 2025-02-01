using Application.Ports.Postgres;
using Application.Ports.S3;
using Application.UseCases.Commands.UploadPhotos;
using Domain.PhotoAggregate;
using Domain.SharedKernel.Errors;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.Commands.UploadPhotos;

[TestSubject(typeof(UploadPhotosHandler))]
public class UploadPhotosHandlerShould
{
    private readonly UploadPhotosCommand _command = new(Guid.NewGuid(), Guid.NewGuid(), [1, 2, 3], [1, 2, 3]);
    [Fact]
    public async Task ReturnOk()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureS3Storage(true);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task ReturnObjectStorageUnavailable()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureS3Storage(false);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
        Assert.True(response.Errors[0].GetType() == typeof(ObjectStorageUnavailable));
    }
    
    private class HandlerBuilder
    {
        private readonly Mock<IPhotoRepository> _photoRepositoryMock = new();
        private readonly Mock<IS3Storage> _s3StorageMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public UploadPhotosHandler Build() =>
            new(_photoRepositoryMock.Object, _s3StorageMock.Object, _unitOfWorkMock.Object);

        public void ConfigureS3Storage(bool savePhotosShouldReturn) => _s3StorageMock
            .Setup(x => x.SavePhotos(It.IsAny<Photo>())).ReturnsAsync(savePhotosShouldReturn);
    }
}
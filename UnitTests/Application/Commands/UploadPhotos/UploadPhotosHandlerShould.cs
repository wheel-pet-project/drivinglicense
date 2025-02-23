using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Application.UseCases.Commands.UploadPhotos;
using Domain.SharedKernel.Errors;
using FluentResults;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.Commands.UploadPhotos;

[TestSubject(typeof(UploadPhotosHandler))]
public class UploadPhotosHandlerShould
{
    private readonly UploadPhotosCommand _command = new(Guid.NewGuid(), Guid.NewGuid(), [1, 2, 3], [1, 2, 3]);
    private readonly Result<(string, string)> _validPhotoKeysResult = Result.Ok(("front", "back"));
    
    [Fact]
    public async Task ReturnOk()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureS3Storage(_validPhotoKeysResult);
        handlerBuilder.ConfigureFormatValidator(true);
        handlerBuilder.ConfigureSizeValidator(true);
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
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureS3Storage(Result.Fail(new ObjectStorageUnavailable("error")));
        handlerBuilder.ConfigureFormatValidator(true);
        handlerBuilder.ConfigureSizeValidator(true);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
        Assert.True(response.Errors[0].GetType() == typeof(ObjectStorageUnavailable));
    }

    [Fact]
    public async Task ReturnFailIfImageFormatIsUnsupported()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureS3Storage(_validPhotoKeysResult);
        handlerBuilder.ConfigureFormatValidator(false);
        handlerBuilder.ConfigureSizeValidator(true);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
    }

    [Fact]
    public async Task ReturnFailIfImageSizeIsToLarge()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureUnitOfWork(Result.Ok());
        handlerBuilder.ConfigureS3Storage(_validPhotoKeysResult);
        handlerBuilder.ConfigureFormatValidator(true);
        handlerBuilder.ConfigureSizeValidator(false);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
    }

    [Fact]
    public async Task ReturnFailIfCommitIsFailed()
    {
        // Arrange
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureUnitOfWork(Result.Fail("error"));
        handlerBuilder.ConfigureS3Storage(_validPhotoKeysResult);
        handlerBuilder.ConfigureFormatValidator(true);
        handlerBuilder.ConfigureSizeValidator(false);
        var handler = handlerBuilder.Build();

        // Act
        var response = await handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsFailed);
    }

    private class HandlerBuilder
    {
        private readonly Mock<IPhotoRepository> _photoRepositoryMock = new();
        private readonly Mock<IS3Storage> _s3StorageMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IImageFormatValidator> _imageFormatValidatorMock = new();
        private readonly Mock<IImageSizeValidator> _imageSizeValidatorMock = new();

        public UploadPhotosHandler Build()
        {
            return new(_photoRepositoryMock.Object, _s3StorageMock.Object, _imageFormatValidatorMock.Object,
                _imageSizeValidatorMock.Object, _unitOfWorkMock.Object);
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn) =>
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);

        public void ConfigureS3Storage(Result<(string frontPhotoKey, string backPhotoKey)> savePhotosShouldReturn)
        {
            _s3StorageMock
                .Setup(x => x.SavePhotos(It.IsAny<List<byte>>(), It.IsAny<List<byte>>()))
                .ReturnsAsync(savePhotosShouldReturn);
        }

        public void ConfigureFormatValidator(bool isSupportedFormatShouldReturn)
        {
            _imageFormatValidatorMock.Setup(x => x.IsSupportedFormat(It.IsAny<List<byte>>()))
                .Returns(isSupportedFormatShouldReturn);
        }

        public void ConfigureSizeValidator(bool isSupportedSizeShouldReturn)
        {
            _imageSizeValidatorMock.Setup(x => x.IsSupportedSize(It.IsAny<int>())).Returns(isSupportedSizeShouldReturn);
        }
    }
}
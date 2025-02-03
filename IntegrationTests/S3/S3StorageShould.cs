using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.PhotoAggregate;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.S3;
using Infrastructure.Options;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IntegrationTests.S3;

[TestSubject(typeof(S3Storage))]
public class S3StorageShould : IntegrationTestBase
{
    private readonly Photo _photo = Photo.Create(Guid.NewGuid(), [1, 2, 3], [4, 5, 6]);
    
    [Fact]
    public async Task AddPhotosBucketNameToPostgres()
    {
        // Arrange
        Context.Photos.Add(_photo);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        var storage = storageBuilder.Build();

        // Act
        await storage.SavePhotos(_photo);

        // Assert
        var photosBucket = await Context.S3Buckets.FirstAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(photosBucket);
        Assert.NotNull(photosBucket.Bucket);
    }

    [Fact]
    public async Task ReturnSuccessForSave()
    {
        // Arrange
        Context.Photos.Add(_photo);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.SavePhotos(_photo);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task ReturnPhotosIfExists()
    {
        // Arrange
        const string bucketName = "test_bucket";
        byte[] frontPhotoBytesExpected = [1, 2, 3];
        byte[] backPhotoBytesExpected = [4, 5, 6];
        
        await Context.Photos.AddAsync(_photo, TestContext.Current.CancellationToken);
        await Context.S3Buckets.AddAsync(new S3BucketModel { Bucket = bucketName, PhotoId = _photo.Id },
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        storageBuilder.ConfigureAmazonS3(bucketName: bucketName,
            getObjectForFrontPhotoShouldReturn: (frontPhotoStorageId: _photo.FrontPhotoStorageId,
                frontPhotoBytesExpected),
            getObjectForBackPhotoShouldReturn: (backPhotoStorageId: _photo.BackPhotoStorageId, backPhotoBytesExpected));
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.GetPhotos(_photo.Id, _photo.FrontPhotoStorageId, _photo.BackPhotoStorageId);

        // Assert
        Assert.Equal(frontPhotoBytesExpected, actual!.Value.frontPhotoBytes);
        Assert.Equal(backPhotoBytesExpected, actual.Value.backPhotoBytes);
    }

    [Fact]
    public async Task ReturnNullIfPhotosNotFound()
    {
        // Arrange
        const string bucketName = "test_bucket";
        
        await Context.Photos.AddAsync(_photo, TestContext.Current.CancellationToken);
        await Context.S3Buckets.AddAsync(new S3BucketModel { Bucket = bucketName, PhotoId = _photo.Id },
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        storageBuilder.ConfigureAmazonS3(bucketName: bucketName,
            getObjectForFrontPhotoShouldReturn: (frontPhotoStorageId: _photo.FrontPhotoStorageId, []),
            getObjectForBackPhotoShouldReturn: (backPhotoStorageId: _photo.BackPhotoStorageId, []),
            statusCode: HttpStatusCode.NotFound);
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.GetPhotos(_photo.Id, _photo.FrontPhotoStorageId, _photo.BackPhotoStorageId);

        // Assert
        Assert.Null(actual);
    }
    
    private class StorageBuilder
    {
        private readonly Mock<IAmazonS3> _s3Mock = new();
        private readonly Mock<Microsoft.Extensions.Logging.ILogger<S3Storage>> _loggerMock = new();
        private IOptions<S3Options> _s3Options = Options.Create(new S3Options { Buckets = ["test_bucket"] });
        private DataContext _context = null!;
        
        public IS3Storage Build(string? bucketName = null)
        {
            _s3Options = Options.Create(new S3Options { Buckets = [bucketName ?? "test_bucket"] });
            return new S3Storage(_s3Mock.Object, _context, _s3Options, _loggerMock.Object);
        }

        public void ConfigureContext(DataContext context)
        {
            _context = context;
        }

        public void ConfigureAmazonS3(string bucketName, 
            (Guid frontPhotoStorageId, byte[] frontPhotoBytes) getObjectForFrontPhotoShouldReturn, 
            (Guid backPhotoStorageId, byte[] backPhotoBytes) getObjectForBackPhotoShouldReturn,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var frontPhotoResponse = new GetObjectResponse
            {
                Key = getObjectForFrontPhotoShouldReturn.frontPhotoStorageId.ToString(),
                BucketName = bucketName,
                ResponseStream = new MemoryStream(getObjectForFrontPhotoShouldReturn.frontPhotoBytes),
                ContentLength = getObjectForFrontPhotoShouldReturn.frontPhotoBytes.Length,
                HttpStatusCode = statusCode
            };
            var backPhotoResponse = new GetObjectResponse
            {
                Key = getObjectForBackPhotoShouldReturn.backPhotoStorageId.ToString(),
                BucketName = bucketName,
                ResponseStream = new MemoryStream(getObjectForBackPhotoShouldReturn.backPhotoBytes),
                ContentLength = getObjectForBackPhotoShouldReturn.backPhotoBytes.Length,
                HttpStatusCode = statusCode
            };
            _s3Mock.Setup(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(r =>
                        r.Key == getObjectForFrontPhotoShouldReturn.frontPhotoStorageId.ToString() &&
                        r.BucketName == bucketName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(frontPhotoResponse);

            _s3Mock.Setup(x =>
                    x.GetObjectAsync(
                        It.Is<GetObjectRequest>(r =>
                            r.Key == getObjectForBackPhotoShouldReturn.backPhotoStorageId.ToString() &&
                            r.BucketName == bucketName),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(backPhotoResponse);
        }
    }
}
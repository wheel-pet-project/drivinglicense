using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate;
using Domain.SharedKernel.ValueObjects;
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
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(
        accountId: Guid.NewGuid(), 
        categoryList: CategoryList.Create([CategoryList.BCategory]),
        number: DrivingLicenseNumber.Create(input: "1234 567891"), 
        name: Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), 
        cityOfBirth: City.Create("Москва"),
        dateOfBirth: new DateOnly(year: 1990, month: 1, day: 1), 
        dateOfIssue: new DateOnly(year: 2020, month: 1, day: 1), 
        codeOfIssue: CodeOfIssue.Create(input: "1234"), 
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1), 
        TimeProvider.System);
    private readonly byte[] photoBytes = [1, 2, 3];
    
    [Fact]
    public async Task AddPhotosBucketNameToPostgres()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, photoBytes, photoBytes);
        
        await AddDrivingLicense(_drivingLicense);
        
        Context.Photos.Add(photo);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var uow = new Infrastructure.Adapters.Postgres.UnitOfWork(Context);
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        var storage = storageBuilder.Build();

        // Act
        await storage.SavePhotos(photo);
        await uow.Commit();

        // Assert
        var photosBucket = await Context.S3Buckets.FirstAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(photosBucket);
        Assert.NotNull(photosBucket.Bucket);
    }

    [Fact]
    public async Task ReturnSuccessForSave()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, photoBytes, photoBytes);
        
        await AddDrivingLicense(_drivingLicense);
        
        Context.Photos.Add(photo);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.SavePhotos(photo);

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
        
        var photo = Photo.Create(_drivingLicense.Id, photoBytes, photoBytes);
        
        await AddDrivingLicense(_drivingLicense);
        
        await Context.Photos.AddAsync(photo, TestContext.Current.CancellationToken);
        await Context.S3Buckets.AddAsync(new S3BucketModel { 
                Id = Guid.NewGuid(), 
                Bucket = bucketName, 
                PhotoId = photo.Id },
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        storageBuilder.ConfigureAmazonS3(bucketName: bucketName,
            getObjectForFrontPhotoShouldReturn: (frontPhotoStorageId: photo.FrontPhotoStorageId,
                frontPhotoBytesExpected),
            getObjectForBackPhotoShouldReturn: (backPhotoStorageId: photo.BackPhotoStorageId, backPhotoBytesExpected));
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.GetPhotos(photo.Id, photo.FrontPhotoStorageId, photo.BackPhotoStorageId);

        // Assert
        Assert.Equal(frontPhotoBytesExpected, actual!.Value.frontPhotoBytes);
        Assert.Equal(backPhotoBytesExpected, actual.Value.backPhotoBytes);
    }

    [Fact]
    public async Task ReturnNullIfPhotosNotFound()
    {
        // Arrange
        const string bucketName = "test_bucket";
        
        var photo = Photo.Create(_drivingLicense.Id, photoBytes, photoBytes);
        
        await AddDrivingLicense(_drivingLicense);
        
        await Context.Photos.AddAsync(photo, TestContext.Current.CancellationToken);
        await Context.S3Buckets.AddAsync(new S3BucketModel { 
                Id = Guid.NewGuid(), 
                Bucket = bucketName, 
                PhotoId = photo.Id },
            TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var storageBuilder = new StorageBuilder();
        storageBuilder.ConfigureContext(Context);
        storageBuilder.ConfigureAmazonS3(bucketName: bucketName,
            getObjectForFrontPhotoShouldReturn: (frontPhotoStorageId: photo.FrontPhotoStorageId, []),
            getObjectForBackPhotoShouldReturn: (backPhotoStorageId: photo.BackPhotoStorageId, []),
            statusCode: HttpStatusCode.NotFound);
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.GetPhotos(photo.Id, photo.FrontPhotoStorageId, photo.BackPhotoStorageId);

        // Assert
        Assert.Null(actual);
    }
    
    private async Task AddDrivingLicense(DrivingLicense drivingLicense)
    {
        Context.Attach(drivingLicense.Status);
        Context.Attach(drivingLicense.CategoryList);
        
        await Context.AddAsync(drivingLicense);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
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
                Key = $"{bucketName}/{getObjectForFrontPhotoShouldReturn.frontPhotoStorageId}.jpg",
                BucketName = bucketName,
                ResponseStream = new MemoryStream(getObjectForFrontPhotoShouldReturn.frontPhotoBytes),
                ContentLength = getObjectForFrontPhotoShouldReturn.frontPhotoBytes.Length,
                HttpStatusCode = statusCode
            };
            var backPhotoResponse = new GetObjectResponse
            {
                Key = $"{bucketName}/{getObjectForBackPhotoShouldReturn.backPhotoStorageId}.jpg",
                BucketName = bucketName,
                ResponseStream = new MemoryStream(getObjectForBackPhotoShouldReturn.backPhotoBytes),
                ContentLength = getObjectForBackPhotoShouldReturn.backPhotoBytes.Length,
                HttpStatusCode = statusCode
            };
            _s3Mock.Setup(x => x.GetObjectAsync(
                    It.Is<GetObjectRequest>(r =>
                        r.Key == $"{bucketName}/{getObjectForFrontPhotoShouldReturn.frontPhotoStorageId}.jpg" &&
                        r.BucketName == bucketName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(frontPhotoResponse);

            _s3Mock.Setup(x =>
                    x.GetObjectAsync(
                        It.Is<GetObjectRequest>(r =>
                            r.Key == $"{bucketName}/{getObjectForBackPhotoShouldReturn.backPhotoStorageId}.jpg" &&
                            r.BucketName == bucketName),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(backPhotoResponse);
        }
    }
}
using Amazon.S3;
using Application.Ports.S3;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.S3;
using Infrastructure.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IntegrationTests.S3;

[TestSubject(typeof(S3Storage))]
public class S3StorageShould : IntegrationTestBase
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(
        Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]),
        DrivingLicenseNumber.Create("1234 567891"),
        Name.Create("Иван", "Иванов", "Иванович"),
        City.Create("Москва"),
        new DateOnly(1990, 1, 1),
        new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"),
        new DateOnly(2030, 1, 1),
        TimeProvider.System);

    private readonly List<byte> _frontPhotoBytes = [1, 2, 3];
    private readonly List<byte> _backPhotoBytes = [1, 2, 3];

    [Fact]
    public async Task ReturnSuccessResult()
    {
        // Arrange
        var storageBuilder = new StorageBuilder();
        var storage = storageBuilder.Build();

        // Act
        var actual = await storage.SavePhotos(_frontPhotoBytes, _backPhotoBytes);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    private class StorageBuilder
    {
        private readonly Mock<IAmazonS3> _s3Mock = new();
        private readonly Mock<Microsoft.Extensions.Logging.ILogger<S3Storage>> _loggerMock = new();
        private IOptions<S3Options> _s3Options = Options.Create(new S3Options { Buckets = ["test_bucket"] });

        public IS3Storage Build(string? bucketName = null)
        {
            _s3Options = Options.Create(new S3Options { Buckets = [bucketName ?? "test_bucket"] });
            return new S3Storage(_s3Mock.Object, _s3Options, _loggerMock.Object);
        }
    }
}
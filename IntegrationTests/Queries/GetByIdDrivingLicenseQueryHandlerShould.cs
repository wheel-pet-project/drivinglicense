using Application.Ports.S3;
using Application.UseCases.Queries.GetByIdDrivingLicense;
using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Moq;
using Npgsql;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetByIdDrivingLicenseQueryHandler))]
public class GetByIdDrivingLicenseQueryHandlerShould : IntegrationTestBase
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]),
        DrivingLicenseNumber.Create("1234 567891"), Name.Create("Иван", "Иванов", "Иванович"),
        City.Create("Москва"), new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1), 
        TimeProvider.System);
    private readonly byte[] _frontPhotoBytes = [1, 2, 3];
    private readonly byte[] _backPhotoBytes = [4, 5, 6];
    
    [Fact]
    public async Task ReturnLicenseWithPhotos()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, _frontPhotoBytes, _backPhotoBytes);
        
        Context.Attach(_drivingLicense.Status);
        Context.Attach(_drivingLicense.CategoryList);
        await Context.AddAsync(_drivingLicense, TestContext.Current.CancellationToken);
        await Context.AddAsync(photo, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        queryHandlerBuilder.ConfigureS3Storage(getPhotosShouldReturn: (_frontPhotoBytes, _backPhotoBytes));
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(new GetByIdDrivingLicenseQuery(Guid.NewGuid(), _drivingLicense.Id),
            TestContext.Current.CancellationToken);
        
        // Assert
        var responseLicense = response.Value;
        Assert.NotNull(responseLicense);
        Assert.Equal(_drivingLicense.Id, responseLicense.DrivingLicenseView.Id);
        Assert.Equal(_drivingLicense.Status, responseLicense.DrivingLicenseView.Status);
        Assert.Equal(_drivingLicense.AccountId, responseLicense.DrivingLicenseView.AccountId);
        Assert.Equal(string.Join(' ', _drivingLicense.Name.FirstName, _drivingLicense.Name.LastName, 
                _drivingLicense.Name.Patronymic), 
            responseLicense.DrivingLicenseView.Name);
        Assert.Equal(_drivingLicense.CategoryList.Categories, responseLicense.DrivingLicenseView.CategoryList);
        Assert.Equal(_drivingLicense.Number.Value, responseLicense.DrivingLicenseView.Number);
        Assert.Equal(_drivingLicense.CityOfBirth.Name, responseLicense.DrivingLicenseView.CityOfBirth);
        Assert.Equal(_drivingLicense.DateOfBirth, responseLicense.DrivingLicenseView.DateOfBirth);
        Assert.Equal(_drivingLicense.CodeOfIssue.Value, responseLicense.DrivingLicenseView.CodeOfIssue);
        Assert.Equal(_drivingLicense.DateOfIssue, responseLicense.DrivingLicenseView.DateOfIssue);
        Assert.Equal(_drivingLicense.DateOfExpiry, responseLicense.DrivingLicenseView.DateOfExpiry);
        Assert.Equal(_frontPhotoBytes, responseLicense.DrivingLicenseView.FrontPhotoBytes);
        Assert.Equal(_backPhotoBytes, responseLicense.DrivingLicenseView.BackPhotoBytes);
    }

    [Fact]
    public async Task ReturnLicenseWithoutPhotos()
    {
         // Arrange
        Context.Attach(_drivingLicense.Status);
        Context.Attach(_drivingLicense.CategoryList);
        await Context.AddAsync(_drivingLicense, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        queryHandlerBuilder.ConfigureS3Storage(getPhotosShouldReturn: null);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(new GetByIdDrivingLicenseQuery(Guid.NewGuid(), _drivingLicense.Id),
            TestContext.Current.CancellationToken);
        
        // Assert
        var responseLicense = response.Value;
        Assert.NotNull(responseLicense);
        Assert.Null(responseLicense.DrivingLicenseView.FrontPhotoBytes);
        Assert.Null(responseLicense.DrivingLicenseView.BackPhotoBytes);
    }

    private class QueryHandlerBuilder
    {
        private readonly Mock<IS3Storage> _s3StorageMock = new();
        private NpgsqlDataSource _dataSource = null!;

        public GetByIdDrivingLicenseQueryHandler Build() => new(_dataSource, _s3StorageMock.Object);

        public void ConfigureDataSource(NpgsqlDataSource dataSource) => _dataSource = dataSource;

        public void ConfigureS3Storage((byte[] frontPhoto, byte[] backPhoto)? getPhotosShouldReturn) => _s3StorageMock
            .Setup(x => x.GetPhotos(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(getPhotosShouldReturn);
    }
}
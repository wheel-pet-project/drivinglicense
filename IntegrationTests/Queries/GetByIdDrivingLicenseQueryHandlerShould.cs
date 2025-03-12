using Application.UseCases.Queries.DapperMappingExtensions;
using Application.UseCases.Queries.GetByIdDrivingLicense;
using Dapper;
using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
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

    private readonly string _frontPhotoKey = "front_key";
    private readonly string _backPhotoKey = "back_key";

    [Fact]
    public async Task ReturnLicenseWithPhotos()
    {
        // Arrange
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        var photo = Photos.Create(_drivingLicense.Id, _frontPhotoKey, _backPhotoKey);

        await AddDrivingLicenseAndPhoto(photo);

        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(new GetByIdDrivingLicenseQuery(_drivingLicense.Id),
            TestContext.Current.CancellationToken);

        // Assert
        var responseLicense = response.Value;
        Assert.NotNull(responseLicense);
        Assert.Equal(_drivingLicense.Id, responseLicense.DrivingLicense.Id);
        Assert.Equal(_drivingLicense.Status, responseLicense.DrivingLicense.Status);
        Assert.Equal(_drivingLicense.AccountId, responseLicense.DrivingLicense.AccountId);
        Assert.Equal(string.Join(' ', _drivingLicense.Name.FirstName, _drivingLicense.Name.LastName,
                _drivingLicense.Name.Patronymic),
            responseLicense.DrivingLicense.Name);
        Assert.Equal(_drivingLicense.CategoryList.Categories, responseLicense.DrivingLicense.CategoryList);
        Assert.Equal(_drivingLicense.Number.Value, responseLicense.DrivingLicense.Number);
        Assert.Equal(_drivingLicense.CityOfBirth.Name, responseLicense.DrivingLicense.CityOfBirth);
        Assert.Equal(_drivingLicense.DateOfBirth, responseLicense.DrivingLicense.DateOfBirth);
        Assert.Equal(_drivingLicense.CodeOfIssue.Value, responseLicense.DrivingLicense.CodeOfIssue);
        Assert.Equal(_drivingLicense.DateOfIssue, responseLicense.DrivingLicense.DateOfIssue);
        Assert.Equal(_drivingLicense.DateOfExpiry, responseLicense.DrivingLicense.DateOfExpiry);
        Assert.Contains(_frontPhotoKey, responseLicense.DrivingLicense.FrontPhotoS3Url);
        Assert.Contains(_backPhotoKey, responseLicense.DrivingLicense.BackPhotoS3Url);
    }

    [Fact]
    public async Task ReturnLicenseWithoutPhotos()
    {
        // Arrange
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        
        Context.Attach(_drivingLicense.Status);
        Context.Attach(_drivingLicense.CategoryList);
        await Context.AddAsync(_drivingLicense, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(new GetByIdDrivingLicenseQuery(_drivingLicense.Id),
            TestContext.Current.CancellationToken);

        // Assert
        var responseLicense = response.Value;
        Assert.NotNull(responseLicense);
        Assert.Null(responseLicense.DrivingLicense.FrontPhotoS3Url);
        Assert.Null(responseLicense.DrivingLicense.BackPhotoS3Url);
    }

    private async Task AddDrivingLicenseAndPhoto(Photos photo)
    {
        Context.Attach(_drivingLicense.Status);
        Context.Attach(_drivingLicense.CategoryList);
        await Context.AddAsync(_drivingLicense, TestContext.Current.CancellationToken);
        await Context.AddAsync(photo, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private class QueryHandlerBuilder
    {
        private NpgsqlDataSource _dataSource = null!;

        public GetByIdDrivingLicenseQueryHandler Build()
        {
            return new GetByIdDrivingLicenseQueryHandler(_dataSource, "https://storage.yandexcloud.net");
        }

        public void ConfigureDataSource(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }
    }
}
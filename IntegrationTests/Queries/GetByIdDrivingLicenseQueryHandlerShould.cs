using System.Data.Common;
using Application.UseCases.Queries.GetByIdDrivingLicense;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetByIdDrivingLicenseQueryHandler))]
public class GetByIdDrivingLicenseQueryHandlerShould : IntegrationTestBase
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]),
        DrivingLicenseNumber.Create("1234 567891"), Name.Create("Иван", "Иванов", "Иванович"),
        City.Create("Москва"), new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1));
    
    [Fact]
    public async Task GetById()
    {
        // Arrange
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
        Assert.Equal(responseLicense.DrivingLicenseView.Id, _drivingLicense.Id);
        Assert.Equal(responseLicense.DrivingLicenseView.Status, _drivingLicense.Status);
        Assert.Equal(responseLicense.DrivingLicenseView.AccountId, _drivingLicense.AccountId);
        Assert.Equal(responseLicense.DrivingLicenseView.Name,
            string.Join(' ', _drivingLicense.Name.FirstName, _drivingLicense.Name.LastName,
                _drivingLicense.Name.Patronymic));
        Assert.Equal(responseLicense.DrivingLicenseView.CategoryList, _drivingLicense.CategoryList.Categories);
        Assert.Equal(responseLicense.DrivingLicenseView.Number, _drivingLicense.Number.Value);
        Assert.Equal(responseLicense.DrivingLicenseView.CityOfBirth, _drivingLicense.CityOfBirth.Name);
        Assert.Equal(responseLicense.DrivingLicenseView.DateOfBirth, _drivingLicense.DateOfBirth);
        Assert.Equal(responseLicense.DrivingLicenseView.CodeOfIssue, _drivingLicense.CodeOfIssue.Value);
        Assert.Equal(responseLicense.DrivingLicenseView.DateOfIssue, _drivingLicense.DateOfIssue);
        Assert.Equal(responseLicense.DrivingLicenseView.DateOfExpiry, _drivingLicense.DateOfExpiry);
    }

    private class QueryHandlerBuilder
    {
        private DbDataSource _dataSource = null!;

        public GetByIdDrivingLicenseQueryHandler Build() => new(_dataSource);

        public void ConfigureDataSource(DbDataSource dataSource) => _dataSource = dataSource;
    }
}
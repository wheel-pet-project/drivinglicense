using System.Data.Common;
using Application.UseCases.Queries.GetAllDrivingLicenses;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetAllDrivingLicensesQueryHandler))]
public class GetAllDrivingLicensesQueryHandlerShould : IntegrationTestBase
{
    private readonly IReadOnlyList<DrivingLicense> _drivingLicenses = 
    [
        DrivingLicense.Create(Guid.NewGuid(), CategoryList.Create([CategoryList.BCategory]),
            DrivingLicenseNumber.Create("1234 567891"), Name.Create("Иван", "Иванов", "Иванович"), 
            City.Create("Москва"), new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1)),
        DrivingLicense.Create(Guid.NewGuid(), CategoryList.Create([CategoryList.BCategory]),
            DrivingLicenseNumber.Create("1234 567892"), Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
            new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1))
    ];

    [Fact]
    public async Task ReturnDrivingLicenseList()
    {
        // Arrange
        Context.Attach(_drivingLicenses[0].Status);
        Context.Attach(_drivingLicenses[1].Status);
        Context.Attach(_drivingLicenses[0].CategoryList);
        Context.Attach(_drivingLicenses[1].CategoryList);
        
        await Context.AddRangeAsync(_drivingLicenses, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(
                new GetAllDrivingLicensesQuery(1, 2), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.DrivingLicenses);
        Assert.Equal(_drivingLicenses.Count, response.DrivingLicenses.Count);
        Assert.NotNull(response.DrivingLicenses[0].Status);
        Assert.NotNull(response.DrivingLicenses[0].Name);
        Assert.NotEqual(Guid.Empty, response.DrivingLicenses[0].Id);
        Assert.NotEqual(Guid.Empty, response.DrivingLicenses[0].AccountId);
    }

    [Fact]
    public async Task ReturnEmptyListIfLicenseNotFound()
    {
        // Arrange
        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(
            new GetAllDrivingLicensesQuery(1, 2), TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.DrivingLicenses);
    }

    [Fact]
    public async Task ReturnDrivingLicenseListWithFiltering()
    {
        // Arrange
        _drivingLicenses[0].MarkAsPendingProcessing();
        
        Context.Attach(_drivingLicenses[0].Status);
        Context.Attach(_drivingLicenses[1].Status);
        Context.Attach(_drivingLicenses[0].CategoryList);
        Context.Attach(_drivingLicenses[1].CategoryList);
        
        await Context.AddRangeAsync(_drivingLicenses, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(
            new GetAllDrivingLicensesQuery(1, 2, Status.PendingProcessing), 
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.DrivingLicenses);
        Assert.Single(response.DrivingLicenses);
        Assert.Equivalent(_drivingLicenses[0].Status, response.DrivingLicenses[0].Status);
    }
    
    private class QueryHandlerBuilder
    {
        private DbDataSource _dataSource = null!;
        
        public GetAllDrivingLicensesQueryHandler Build() => new GetAllDrivingLicensesQueryHandler(_dataSource);
        
        public void ConfigureDataSource(DbDataSource dataSource) => _dataSource = dataSource;
    }
}
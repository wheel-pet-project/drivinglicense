using Application.UseCases.Queries.GetAllDrivingLicenses;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Npgsql;
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
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1), 
            TimeProvider.System),
        DrivingLicense.Create(Guid.NewGuid(), CategoryList.Create([CategoryList.BCategory]),
            DrivingLicenseNumber.Create("1234 567892"), Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
            new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1), 
            TimeProvider.System)
    ];
    
    [Fact]
    public async Task ReturnEmptyListIfLicenseNotFound()
    {
        // Arrange
        var queryHandlerBuilder = new QueryHandlerBuilder();
        queryHandlerBuilder.ConfigureDataSource(DataSource);
        var handler = queryHandlerBuilder.Build();

        // Act
        var response = await handler.Handle(
            new GetAllDrivingLicensesQuery(Guid.NewGuid(), 1, 2, Status.Approved),
            TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Value.DrivingLicenses);
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
            new GetAllDrivingLicensesQuery(Guid.NewGuid(), 1, 2, Status.PendingProcessing), 
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Value.DrivingLicenses);
        Assert.Single(response.Value.DrivingLicenses);
        Assert.Equivalent(_drivingLicenses[0].Status, response.Value.DrivingLicenses[0].Status);
    }
    
    private class QueryHandlerBuilder
    {
        private NpgsqlDataSource _dataSource = null!;
        
        public GetAllDrivingLicensesQueryHandler Build() => new(_dataSource);
        
        public void ConfigureDataSource(NpgsqlDataSource dataSource) => _dataSource = dataSource;
    }
}
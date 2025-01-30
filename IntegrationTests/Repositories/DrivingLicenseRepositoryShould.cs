using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

[TestSubject(typeof(DrivingLicense))]
public class DrivingLicenseRepositoryShould : IntegrationTestBase
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
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1));
    
    [Fact]
    public async Task Add()
    {
        // Arrange
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repository, uow) = repositoryAndUowBuilder.Build();

        // Act
        await repository.Add(_drivingLicense);
        await uow.Commit();

        // Assert
        var licenseFromDb = await Context.DrivingLicenses.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(licenseFromDb);
        Assert.Equal(_drivingLicense, licenseFromDb);
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        var licenseForUpdate = DrivingLicense.Create(Guid.NewGuid(), 
            CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create(input: "1234 567891"), 
            Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), City.Create("Москва"),
            new DateOnly(year: 1990, month: 1, day: 1), new DateOnly(year: 2020, month: 1, day: 1), 
            CodeOfIssue.Create(input: "1234"), new DateOnly(year: 2030, month: 1, day: 1));
        
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(licenseForUpdate);
        await uowForArrange.Commit();

        var (repository, uow) = repositoryAndUowBuilder.Build();
        
        // Act
        licenseForUpdate.MarkAsPendingProcessing();
        repository.Update(licenseForUpdate);
        await uow.Commit();

        // Assert
        var licenseFromDb = await Context.DrivingLicenses.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(licenseFromDb);
        Assert.Equal(licenseForUpdate, licenseFromDb);
    }

    [Fact]
    public async Task GetById()
    {
        // Arrange
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(_drivingLicense);
        await uowForArrange.Commit();

        var (repository, _) = repositoryAndUowBuilder.Build();
        
        // Act
        var licenseFromDb = await repository.GetById(_drivingLicense.Id);

        // Assert
        Assert.NotNull(licenseFromDb);
        Assert.Equal(_drivingLicense, licenseFromDb);
    }

    [Fact]
    public async Task GetAll()
    {
        // Arrange
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(_drivingLicense);
        await uowForArrange.Commit();

        var (repository, _) = repositoryAndUowBuilder.Build();
        
        // Act
        var licenseFromDb = await repository.GetAll(1, 1);

        // Assert
        Assert.NotNull(licenseFromDb);
        Assert.Equal(_drivingLicense, licenseFromDb[0]);
    }

    [Fact]
    public async Task GetAllWithFilter()
    {
        // Arrange
        var expectedLicense = DrivingLicense.Create(Guid.NewGuid(), 
            CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create(input: "1234 567891"), 
            Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), City.Create("Москва"),
            new DateOnly(year: 1990, month: 1, day: 1), new DateOnly(year: 2020, month: 1, day: 1), 
            CodeOfIssue.Create(input: "1234"), new DateOnly(year: 2030, month: 1, day: 1));
        expectedLicense.MarkAsPendingProcessing();
        
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(_drivingLicense);
        await repositoryForArrange.Add(expectedLicense);
        await uowForArrange.Commit();

        var (repository, _) = repositoryAndUowBuilder.Build();
        
        // Act
        var licenseFromDb = await repository.GetAll(1, 1, x => x.Status == Status.PendingProcessing);

        // Assert
        Assert.NotNull(licenseFromDb);
        Assert.Equal(expectedLicense, licenseFromDb[0]);
    }
    
    private class RepositoryAndUnitOfWorkBuilder
    {
        private DataContext _context = null!;
        
        public (IDrivingLicenseRepository repostory, IUnitOfWork uow) Build() => 
            (new DrivingLicenseRepository(_context), new Infrastructure.Adapters.Postgres.UnitOfWork(_context));

        public void ConfigureContext(DataContext context) => _context = context;
    }
}
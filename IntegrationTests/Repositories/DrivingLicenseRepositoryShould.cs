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
            CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create("1234 567891"),
            Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
            new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1),
            TimeProvider.System);

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
            CategoryList.Create([CategoryList.BCategory]), DrivingLicenseNumber.Create("1234 567891"),
            Name.Create("Иван", "Иванов", "Иванович"), City.Create("Москва"),
            new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), new DateOnly(2030, 1, 1),
            TimeProvider.System);
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

        public (IDrivingLicenseRepository repostory, IUnitOfWork uow) Build()
        {
            return (new DrivingLicenseRepository(_context), new Infrastructure.Adapters.Postgres.UnitOfWork(_context));
        }

        public void ConfigureContext(DataContext context)
        {
            _context = context;
        }
    }
}
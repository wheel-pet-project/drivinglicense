using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.PhotoAggregate;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

[TestSubject(typeof(PhotoRepository))]
public class PhotoRepositoryShould : IntegrationTestBase
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

    private readonly string _frontPhotoKey = "front_key";
    private readonly string _backPhotoKey = "back_key";

    [Fact]
    public async Task Add()
    {
        // Arrange
        var photo = Photos.Create(_drivingLicense.Id, _frontPhotoKey, _backPhotoKey);


        await AddDrivingLicense(_drivingLicense);

        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repository, uow) = repositoryAndUowBuilder.Build();

        // Act
        await repository.Add(photo);
        await uow.Commit();

        // Assert
        var photoFromDb = await Context.Photos.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(photoFromDb);
        Assert.Equal(photo, photoFromDb);
    }

    [Fact]
    public async Task GetById()
    {
        // Arrange
        var photo = Photos.Create(_drivingLicense.Id, _frontPhotoKey, _backPhotoKey);

        await AddDrivingLicense(_drivingLicense);

        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(photo);
        await uowForArrange.Commit();

        var (repository, uow) = repositoryAndUowBuilder.Build();

        // Act
        var actual = await repository.GetById(photo.Id);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(photo, actual);
    }

    private async Task AddDrivingLicense(DrivingLicense drivingLicense)
    {
        Context.Attach(drivingLicense.Status);
        Context.Attach(drivingLicense.CategoryList);

        await Context.AddAsync(drivingLicense);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private class RepositoryAndUnitOfWorkBuilder
    {
        private DataContext _context = null!;

        public (IPhotoRepository, IUnitOfWork) Build()
        {
            return (new PhotoRepository(_context), new Infrastructure.Adapters.Postgres.UnitOfWork(_context));
        }

        public void ConfigureContext(DataContext context)
        {
            _context = context;
        }
    }
}
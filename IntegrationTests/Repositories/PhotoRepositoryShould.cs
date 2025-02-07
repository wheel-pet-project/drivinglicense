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
    private readonly byte[] _photoBytes = [1, 2, 3];
    
    [Fact]
    public async Task Add()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, _photoBytes, _photoBytes);
        
        
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
    public async Task Delete()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, _photoBytes, _photoBytes);

        await AddDrivingLicense(_drivingLicense);
        
        var repositoryAndUowBuilder = new RepositoryAndUnitOfWorkBuilder();
        repositoryAndUowBuilder.ConfigureContext(Context);
        var (repositoryForArrange, uowForArrange) = repositoryAndUowBuilder.Build();
        await repositoryForArrange.Add(photo);
        await uowForArrange.Commit();
        
        var (repository, uow) = repositoryAndUowBuilder.Build();

        // Act
        repository.Delete(photo);
        await uow.Commit();

        // Assert
        var photoFromDb = await Context.Photos.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.Null(photoFromDb);
    }

    [Fact]
    public async Task GetById()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense.Id, _photoBytes, _photoBytes);
        
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

        public (IPhotoRepository, IUnitOfWork) Build() => 
            (new PhotoRepository(_context), new Infrastructure.Adapters.Postgres.UnitOfWork(_context));

        public void ConfigureContext(DataContext context) => _context = context;
    }
}
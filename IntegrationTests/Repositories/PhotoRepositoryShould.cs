using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.PhotoAggregate;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Ports.Postgres;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

public class PhotoRepositoryShould : IntegrationTestBase
{
    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(
        accountId: Guid.NewGuid(), 
        categoryList: CategoryList.Create([CategoryList.BCategory]),
        number: DrivingLicenseNumber.Create(input: "1234 567891"), 
        name: Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), 
        cityOfBirth: "Москва",
        dateOfBirth: new DateOnly(year: 1990, month: 1, day: 1), 
        dateOfIssue: new DateOnly(year: 2020, month: 1, day: 1), 
        codeOfIssue: CodeOfIssue.Create(input: "1234"), 
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1));
    private readonly byte[] _photoBytes = [1, 2, 3];
    
    [Fact]
    public async Task Add()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicense, _photoBytes, _photoBytes);

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
        var photo = Photo.Create(_drivingLicense, _photoBytes, _photoBytes);

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
        var photo = Photo.Create(_drivingLicense, _photoBytes, _photoBytes);

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
    
    private class RepositoryAndUnitOfWorkBuilder
    {
        private DataContext _context = null!;

        public (IPhotoRepository, IUnitOfWork) Build() => 
            (new PhotoRepository(_context), new Infrastructure.Adapters.Postgres.UnitOfWork(_context));

        public void ConfigureContext(DataContext context) => _context = context;
    }
}
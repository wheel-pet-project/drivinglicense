using Core.Domain.PhotoAggregate;
using Core.Ports.Postgres;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Repositories;

[TestSubject(typeof(PhotoRepository))]
public class PhotoRepositoryShould : IntegrationTestBase
{
    private readonly Guid _drivingLicenseId = Guid.NewGuid();
    private readonly byte[] _photoBytes = [1, 2, 3];
    
    [Fact]
    public async Task Add()
    {
        // Arrange
        var photo = Photo.Create(_drivingLicenseId, _photoBytes, _photoBytes);

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
        var photo = Photo.Create(_drivingLicenseId, _photoBytes, _photoBytes);

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
        var photo = Photo.Create(_drivingLicenseId, _photoBytes, _photoBytes);

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
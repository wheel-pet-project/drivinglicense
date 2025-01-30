using Application.Ports.Postgres;
using Domain.PhotoAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo?> GetById(Guid id)
    {
        return await context.Photos.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Add(Photo photo)
    {
        await context.Photos.AddAsync(photo);
    }

    public void Delete(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
using Core.Domain.PhotoAggregate;

namespace Core.Ports.Postgres;

public interface IPhotoRepository
{ 
    Task<Photo?> GetById(Guid id);
    
    Task Add(Photo photo);

    void Delete(Photo photo);
}
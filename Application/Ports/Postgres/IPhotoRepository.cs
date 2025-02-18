using Domain.PhotoAggregate;

namespace Application.Ports.Postgres;

public interface IPhotoRepository
{
    Task<Photos?> GetById(Guid id);

    Task Add(Photos drivingLicensePhotos);
}
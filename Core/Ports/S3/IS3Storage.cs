using Core.Domain.PhotoAggregate;

namespace Core.Ports.S3;

public interface IS3Storage
{
    Task<bool> SavePhotos(Photo photo);

    Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?> GetPhotos(Guid frontPhotoId, Guid backPhotoId);
}
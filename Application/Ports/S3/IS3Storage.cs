using Domain.PhotoAggregate;

namespace Application.Ports.S3;

public interface IS3Storage
{
    Task<bool> SavePhotos(Photo photo);

    Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?> GetPhotos(Guid photoId, Guid frontPhotoId, Guid backPhotoId);
}
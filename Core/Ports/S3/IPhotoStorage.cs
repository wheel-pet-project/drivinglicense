using Core.Domain.PhotoAggregate;

namespace Core.Ports.S3;

public interface IPhotoStorage
{
    Task<bool> Save(Photo photo);

    Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?> Get(Guid frontPhotoId, Guid backPhotoId);
}
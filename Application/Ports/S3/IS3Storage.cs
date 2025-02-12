using FluentResults;

namespace Application.Ports.S3;

public interface IS3Storage
{
    Task<Result<(string frontPhotoKeyWithBucket, string backPhotoKeyWithBucket)>> SavePhotos(
        List<byte> frontPhotoBytes,
        List<byte> backPhotoBytes);
}
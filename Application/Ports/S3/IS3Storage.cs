using FluentResults;

namespace Application.Ports.S3;

public interface IS3Storage
{
    Task<Result<(string frontPhotoBucketAndKey, string backPhotoBucketAndKey)>> SavePhotos(
        List<byte> frontPhotoBytes,
        List<byte> backPhotoBytes);
}
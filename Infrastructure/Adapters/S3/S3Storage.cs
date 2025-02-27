using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.SharedKernel.Exceptions.ArgumentException;
using FluentResults;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.S3;

public class S3Storage(
    IAmazonS3 s3Client,
    IOptions<S3Options> options,
    ILogger<S3Storage> logger)
    : IS3Storage
{
    private readonly S3Options _s3Options = options.Value;

    public async Task<Result<(string frontPhotoBucketAndKey, string backPhotoBucketAndKey)>> SavePhotos(
        List<byte> frontPhotoBytes,
        List<byte> backPhotoBytes)
    {
        var rnd = new Random();
        var currentBucket = _s3Options.Buckets[rnd.Next(_s3Options.Buckets.Length)];

        try
        {
            if (frontPhotoBytes is null || backPhotoBytes is null)
                throw new ValueIsRequiredException("Photos are required");

            var frontPhotoBytesArray = frontPhotoBytes.ToArray();
            var backPhotoBytesArray = backPhotoBytes.ToArray();

            var frontPhotoKey = Guid.NewGuid().ToString();
            var backPhotoKey = Guid.NewGuid().ToString();

            var frontPhotoPutRequest = new PutObjectRequest
            {
                BucketName = currentBucket,
                Key = frontPhotoKey,
                ContentType = "image/jpeg",
                InputStream = new MemoryStream(frontPhotoBytesArray),
                ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(frontPhotoBytesArray))
            };
            var backPhotoPutRequest = new PutObjectRequest
            {
                BucketName = currentBucket,
                Key = backPhotoKey,
                ContentType = "image/jpeg",
                InputStream = new MemoryStream(backPhotoBytesArray),
                ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(backPhotoBytesArray))
            };

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest, cancellationToken);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest, cancellationToken);

            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask);

            return Result.Ok(($"{currentBucket}/{frontPhotoKey}", $"{currentBucket}/{backPhotoKey}"));
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError("Could not upload photos to S3 storage, exception : {ex}", ex);
            return Result.Fail("Could not upload photos to S3 storage");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning("Time-out for uploading photos has expired, exception: {ex}", ex);
            return Result.Fail("Time-out for uploading photos has expired.");
        }
    }
}
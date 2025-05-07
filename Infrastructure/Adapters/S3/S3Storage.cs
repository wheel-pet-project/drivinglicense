using System.Net;
using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.SharedKernel.Errors;
using FluentResults;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ArgumentException = System.ArgumentException;

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
        if (frontPhotoBytes is null || backPhotoBytes is null) throw new ArgumentException("Photos are required");
        
        var currentBucket = GetCurrentBucket();
        var frontPhotoKey = CalculatePhotoKey();
        var backPhotoKey = CalculatePhotoKey();

        return await ProcessWithExceptionHandling(async () =>
        {
            var frontPhotoPutRequest = CreateRequest(currentBucket, frontPhotoKey, frontPhotoBytes);
            var backPhotoPutRequest = CreateRequest(currentBucket, backPhotoKey, backPhotoBytes);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest, cts.Token);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest, cts.Token);

            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask);

            return Result.Ok(($"{currentBucket}/{frontPhotoKey}", $"{currentBucket}/{backPhotoKey}"));
        });
    }
    
    private string GetCurrentBucket()
    {
        var rnd = new Random();
        return _s3Options.Buckets[rnd.Next(_s3Options.Buckets.Length)];
    }
    
    private static string CalculatePhotoKey()
    {
        return Guid.NewGuid().ToString();
    }

    private async Task<Result<T>> ProcessWithExceptionHandling<T>(Func<Task<Result<T>>> func)
    {
        try
        {
            return await func();
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError("Could not upload photos to S3 storage, exception : {ex}", ex);

            return ReturnСorrespondingResult<T>(ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning("Time-out for uploading photos has expired, exception: {ex}", ex);

            return ReturnTimeoutResult<T>();
        }
    }
    
    private PutObjectRequest CreateRequest(string currentBucket, string key, List<byte> bytes)
    {
        var bytesArray = bytes.ToArray();

        return new PutObjectRequest
        {
            BucketName = currentBucket,
            Key = key,
            ContentType = "image/jpeg",
            InputStream = new MemoryStream(bytesArray),
            ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(bytesArray))
        };
    }

    private Result<T> ReturnTimeoutResult<T>()
    {
        return Result.Fail("Time-out for uploading photos has expired.");
    }

    private Result<T> ReturnСorrespondingResult<T>(AmazonS3Exception ex)
    {
        return ex.StatusCode > HttpStatusCode.InternalServerError
            ? Result.Fail(new ObjectStorageUnavailable("Object storage unavailable"))
            : Result.Fail("Could not upload photos to S3 storage");
    }
}
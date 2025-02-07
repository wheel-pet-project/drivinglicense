using System.Net;
using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.PhotoAggregate;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.S3;

public class S3Storage(
    IAmazonS3 s3Client, 
    DataContext context,
    IOptions<S3Options> options,
    ILogger<S3Storage> logger) : IS3Storage
{
    private readonly S3Options _s3Options = options.Value;
    
    public async Task<bool> SavePhotos(Photo photo)
    {
        var rnd = new Random();
        var currentBucket = _s3Options.Buckets[rnd.Next(_s3Options.Buckets.Length)];
        
        try
        {
            var frontPhotoPutRequest = new PutObjectRequest
            {
                BucketName = currentBucket,
                Key = $"{currentBucket}/{photo.FrontPhotoStorageId}.jpg",
                ContentType = "image/jpeg",
                InputStream = new MemoryStream(photo.FrontPhotoBytes!),
                ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(photo.FrontPhotoBytes!)),
                ServerSideEncryptionCustomerProvidedKeyMD5 = Convert.ToBase64String(MD5.HashData(photo.FrontPhotoBytes!)),
            };
            var backPhotoPutRequest = new PutObjectRequest
            {
                BucketName = currentBucket,
                Key = $"{currentBucket}/{photo.BackPhotoStorageId}.jpg",
                ContentType = "image/jpeg",
                InputStream = new MemoryStream(photo.BackPhotoBytes!),
                ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(photo.BackPhotoBytes!)),
                ServerSideEncryptionCustomerProvidedKeyMD5 = Convert.ToBase64String(MD5.HashData(photo.BackPhotoBytes!)),
            };
            var s3BucketModel = new S3BucketModel
            {
                Id = Guid.NewGuid(),
                PhotoId = photo.Id,
                Bucket = currentBucket
            };

            await context.S3Buckets.AddAsync(s3BucketModel);

            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest);
            
            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError("Could not upload photos to S3 storage, exception : {ex}", ex);
            return false;
        }
    }

    public async Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?> GetPhotos(
        Guid photoId,
        Guid frontPhotoStorageId,
        Guid backPhotoStorageId)
    {
        var s3Bucket = context.S3Buckets.FirstOrDefault(b => b.PhotoId == photoId);
        if (s3Bucket == null) return null;
        var currentBucket = s3Bucket.Bucket;
        
        var frontPhotoKey = $"{currentBucket}/{frontPhotoStorageId}.jpg";
        var backPhotoKey = $"{currentBucket}/{backPhotoStorageId}.jpg";
        
        var frontPhotoGetRequest = new GetObjectRequest
        {
            BucketName = currentBucket,
            Key = frontPhotoKey
        };
        var backPhotoGetRequest = new GetObjectRequest
        {
            BucketName = currentBucket,
            Key = backPhotoKey
        };
        
        var frontPhotoGettingTask = s3Client.GetObjectAsync(frontPhotoGetRequest);
        var backPhotoGettingTask = s3Client.GetObjectAsync(backPhotoGetRequest);
        
        var getObjectResponses = await Task.WhenAll(frontPhotoGettingTask, backPhotoGettingTask);

        var frontPhotoResult = getObjectResponses.First(x => x.Key == frontPhotoKey);
        var backPhotoResult = getObjectResponses.First(x => x.Key == backPhotoKey);
        
        if (frontPhotoResult.HttpStatusCode != HttpStatusCode.OK || 
            backPhotoResult.HttpStatusCode != HttpStatusCode.OK) 
            return null;
        
        var frontPhotoLength = frontPhotoResult.ContentLength > int.MaxValue
            ? int.MaxValue
            : (int)frontPhotoResult.ContentLength;
        var backPhotoLength = backPhotoResult.ContentLength > int.MaxValue
            ? int.MaxValue
            : (int)backPhotoResult.ContentLength;
        var frontPhoto = new byte[frontPhotoLength];
        var backPhoto = new byte[backPhotoLength];
    
        await frontPhotoResult.ResponseStream.ReadExactlyAsync(frontPhoto.AsMemory(0, frontPhotoLength));
        await backPhotoResult.ResponseStream.ReadExactlyAsync(backPhoto.AsMemory(0, backPhotoLength));

        return (frontPhoto, backPhoto);
    }
}
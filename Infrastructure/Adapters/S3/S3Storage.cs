using System.Net;
using System.Security.Cryptography;
using System.Text;
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
                Key = photo.FrontPhotoStorageId.ToString(),
                ContentBody = Encoding.UTF8.GetString(photo.FrontPhotoBytes!),
                MD5Digest = MD5.HashData(photo.FrontPhotoBytes!).ToString()
            };
            var backPhotoPutRequest = new PutObjectRequest
            {
                BucketName = currentBucket,
                Key = photo.BackPhotoStorageId.ToString(),
                ContentBody = Encoding.UTF8.GetString(photo.BackPhotoBytes!),
                MD5Digest = MD5.HashData(photo.BackPhotoBytes!).ToString()
            };
            var s3BucketModel = new S3BucketModel
            {
                PhotoId = photo.Id,
                Bucket = currentBucket
            };

            await context.S3Buckets.AddAsync(s3BucketModel);

            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest);
            var bucketSaveToPostgresTask = context.SaveChangesAsync();

            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask, bucketSaveToPostgresTask);
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
        
        var frontPhotoGetRequest = new GetObjectRequest
        {
            BucketName = currentBucket,
            Key = frontPhotoStorageId.ToString(),
        };
        var backPhotoGetRequest = new GetObjectRequest
        {
            BucketName = currentBucket,
            Key = backPhotoStorageId.ToString(),
        };
        
        var frontPhotoGettingTask = s3Client.GetObjectAsync(frontPhotoGetRequest);
        var backPhotoGettingTask = s3Client.GetObjectAsync(backPhotoGetRequest);
        
        var getObjectResponses = await Task.WhenAll(frontPhotoGettingTask, backPhotoGettingTask);

        var frontPhotoResult = getObjectResponses.First(x => x.Key == frontPhotoStorageId.ToString());
        var backPhotoResult = getObjectResponses.First(x => x.Key == backPhotoStorageId.ToString());
        
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
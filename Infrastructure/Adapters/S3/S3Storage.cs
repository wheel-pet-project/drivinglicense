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
using Refit;

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
            var frontPhotoPutRequest = new PutObjectRequest()
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
                Id = Guid.NewGuid(),
                Bucket = currentBucket,
                FrontPhotoStorageId = photo.FrontPhotoStorageId,
                BackPhotoStorageId = photo.BackPhotoStorageId
            };

            await context.S3Buckets.AddAsync(s3BucketModel);
            
            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest);
            var bucketSaveToPostgresTask = context.SaveChangesAsync();
            
            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask, bucketSaveToPostgresTask);
            return true;
        }
        catch (AmazonS3Exception)
        {
            logger.LogError("Could not upload photos to S3 storage");
            return false;
        }
    }

    public async Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?> GetPhotos(Guid frontPhotoId, Guid backPhotoId)
    {
        var s3Bucket = context.S3Buckets.FirstOrDefault(b => b.Id == frontPhotoId);
        if (s3Bucket == null) return await new Task<(byte[] frontPhotoBytes, byte[] backPhotoBytes)?>(null);
        
        var currentBucket = s3Bucket.Bucket;
        var frontPhotoGetRequest = new GetObjectRequest()
        {
            BucketName = currentBucket,
            Key = frontPhotoId.ToString(),
        };
        var backPhotoGetRequest = new GetObjectRequest
        {
            BucketName = currentBucket,
            Key = backPhotoId.ToString(),
        };
        
        var frontPhotoGettingTask = s3Client.GetObjectAsync(frontPhotoGetRequest);
        var backPhotoGettingTask = s3Client.GetObjectAsync(backPhotoGetRequest);
        
        var getObjectResponses = await Task.WhenAll(frontPhotoGettingTask, backPhotoGettingTask);

        var frontPhotoResult = getObjectResponses.First(x => x.Key == frontPhotoId.ToString());
        var backPhotoResult = getObjectResponses.First(x => x.Key == backPhotoId.ToString());
        var frontPhotoLength = frontPhotoResult.ContentLength > int.MaxValue
            ? int.MaxValue
            : (int)frontPhotoResult.ContentLength;
        var backPhotoLength = backPhotoResult.ContentLength > int.MaxValue
            ? int.MaxValue
            : (int)backPhotoResult.ContentLength;
        
        var frontPhoto = new byte[frontPhotoLength];
        var backPhoto = new byte[backPhotoLength];
        await frontPhotoResult.ResponseStream.ReadAsync(frontPhoto.AsMemory(0, frontPhotoLength));
        await backPhotoResult.ResponseStream.ReadAsync(backPhoto.AsMemory(0, backPhotoLength));
        
        return (frontPhoto, backPhoto);
    }
}
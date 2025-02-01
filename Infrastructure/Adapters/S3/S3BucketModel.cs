namespace Infrastructure.Adapters.S3;

public class S3BucketModel
{
    public Guid Id { get; init; }
    
    public Guid FrontPhotoStorageId { get; init; }
    
    public Guid BackPhotoStorageId { get; init; }
    
    public string Bucket { get; init; } = null!;
}
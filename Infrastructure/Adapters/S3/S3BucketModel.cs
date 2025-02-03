using Domain.PhotoAggregate;

namespace Infrastructure.Adapters.S3;

public class S3BucketModel
{
    public Guid PhotoId { get; init; }
    
    public string Bucket { get; init; } = null!;
}
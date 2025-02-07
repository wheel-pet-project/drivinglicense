namespace Infrastructure.Adapters.S3;

public class S3BucketModel
{
    public required Guid Id { get; init; }
    public required Guid PhotoId { get; init; }
    
    public required string Bucket { get; init; } = null!;
}
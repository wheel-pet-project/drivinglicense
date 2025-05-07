using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.PublicExceptions;

namespace Domain.PhotoAggregate;

public class Photos : Aggregate
{
    private Photos()
    {
    }

    private Photos(
        Guid drivingLicenseId,
        string frontPhotoStorageBucketAndKey,
        string backPhotoStorageBucketWithKey) : this()
    {
        Id = Guid.NewGuid();
        DrivingLicenseId = drivingLicenseId;
        FrontPhotoStorageBucketAndKey = frontPhotoStorageBucketAndKey;
        BackPhotoStorageBucketWithKey = backPhotoStorageBucketWithKey;
    }

    public Guid Id { get; }
    public Guid DrivingLicenseId { get; }
    public string FrontPhotoStorageBucketAndKey { get; private set; } = null!;
    public string BackPhotoStorageBucketWithKey { get; private set; } = null!;

    public static Photos Create(
        Guid drivingLicenseId,
        string frontPhotoStorageKeyWithBucket,
        string backPhotoStorageKeyWithBucket)
    {
        if (drivingLicenseId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(drivingLicenseId)} cannot be empty");
        if (string.IsNullOrWhiteSpace(frontPhotoStorageKeyWithBucket))
            throw new ValueIsRequiredException($"{nameof(frontPhotoStorageKeyWithBucket)} cannot be null or empty");
        if (string.IsNullOrWhiteSpace(backPhotoStorageKeyWithBucket))
            throw new ValueIsRequiredException($"{nameof(backPhotoStorageKeyWithBucket)} cannot be null or empty");

        var photo = new Photos(drivingLicenseId, frontPhotoStorageKeyWithBucket, backPhotoStorageKeyWithBucket);
        photo.AddDomainEvent(new PhotosAddedDomainEvent(drivingLicenseId));

        return photo;
    }
}
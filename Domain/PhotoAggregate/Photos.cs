using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.PhotoAggregate;

public class Photos : Aggregate
{
    private Photos()
    {
    }

    private Photos(
        Guid drivingLicenseId,
        string frontPhotoStorageKeyWithBucket,
        string backPhotoStorageKeyWithBucket) : this()
    {
        Id = Guid.NewGuid();
        DrivingLicenseId = drivingLicenseId;
        FrontPhotoStorageKeyWithBucket = frontPhotoStorageKeyWithBucket;
        BackPhotoStorageKeyWithBucket = backPhotoStorageKeyWithBucket;
    }

    public Guid Id { get; private set; }
    public Guid DrivingLicenseId { get; private set; }
    public string FrontPhotoStorageKeyWithBucket { get; private set; }
    public string BackPhotoStorageKeyWithBucket { get; private set; }

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

        var newPhoto = new Photos(drivingLicenseId, frontPhotoStorageKeyWithBucket, backPhotoStorageKeyWithBucket);
        newPhoto.AddDomainEvent(new PhotosAddedDomainEvent(drivingLicenseId));

        return newPhoto;
    }
}
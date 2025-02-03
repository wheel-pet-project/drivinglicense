using Domain.PhotoAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.PhotoAggregate;

public class Photo : Aggregate
{
    private Photo(){}

    private Photo(
        Guid drivingLicenseId,
        byte[] frontPhotoBytes, 
        byte[] backPhotoBytes) : this()
    {
        Id = Guid.NewGuid();
        FrontPhotoStorageId = Guid.NewGuid();
        BackPhotoStorageId = Guid.NewGuid();
        DrivingLicenseId = drivingLicenseId;
        FrontPhotoBytes = frontPhotoBytes;
        BackPhotoBytes = backPhotoBytes;
    }
    
    public Guid Id { get; private set; }
    
    public Guid DrivingLicenseId { get; private set; }

    public Guid FrontPhotoStorageId { get; private set; }
    
    public Guid BackPhotoStorageId { get; private set; }
    
    public byte[]? FrontPhotoBytes { get; private set; }
    
    public byte[]? BackPhotoBytes { get; private set; }

    public void AddPhotoAddedDomainEvent() => AddDomainEvent(new PhotoAddedDomainEvent(DrivingLicenseId));
    
    public static Photo Create(
        Guid drivingLicenseId,
        byte[] frontPhotoBytes, 
        byte[] backPhotoBytes)
    {
        if (drivingLicenseId == Guid.Empty) 
            throw new ValueIsRequiredException($"{nameof(drivingLicenseId)} cannot be empty");
        if (frontPhotoBytes is null || frontPhotoBytes.Length == 0)
            throw new ValueIsRequiredException($"{nameof(frontPhotoBytes)} cannot be null or empty");
        if (backPhotoBytes is null || backPhotoBytes.Length == 0)
            throw new ValueIsRequiredException($"{nameof(backPhotoBytes)} cannot be null or empty");

        return new Photo(drivingLicenseId, frontPhotoBytes, backPhotoBytes);
    }
}
using Domain.PhotoAggregate;
using Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.PhotoAggregate;

[TestSubject(typeof(Photos))]
public class PhotoShould
{
    private readonly string _frontPhotoKey = "front";
    private readonly string _backPhotoKey = "back";
    private readonly Guid _drivingLicenseId = Guid.NewGuid();

    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Photos.Create(_drivingLicenseId, _frontPhotoKey, _backPhotoKey);

        // Assert
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(_frontPhotoKey, actual.FrontPhotoStorageKeyWithBucket);
        Assert.Equal(_backPhotoKey, actual.BackPhotoStorageKeyWithBucket);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfDrivingLicenseIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            Photos.Create(Guid.Empty, _frontPhotoKey, _backPhotoKey);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowValueIsRequiredExceptionIfFrontPhotoStorageKeyIsNullOrEmpty(string? invalidPhotoKey)
    {
        // Arrange

        // Act
        void Act()
        {
            Photos.Create(_drivingLicenseId, invalidPhotoKey!, _backPhotoKey);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowValueIsRequiredExceptionIfBackPhotoBytesIsNullOrEmpty(string? invalidPhotoKey)
    {
        // Arrange

        // Act
        void Act()
        {
            Photos.Create(_drivingLicenseId, _frontPhotoKey, invalidPhotoKey!);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddDomainEvent()
    {
        // Arrange

        // Act
        var photo = Photos.Create(_drivingLicenseId, _frontPhotoKey, _backPhotoKey);

        // Assert
        Assert.NotEmpty(photo.DomainEvents);
    }
}
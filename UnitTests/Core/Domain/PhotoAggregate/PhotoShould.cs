using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.PhotoAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.PhotoAggregate;

[TestSubject(typeof(Photo))]
public class PhotoShould
{
    private readonly byte[] _photoBytes = [1, 2, 3];

    private readonly Guid _drivingLicenseId = Guid.NewGuid();
    
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Photo.Create(_drivingLicenseId, _photoBytes, _photoBytes);

        // Assert
        Assert.NotEqual(Guid.Empty, actual.FrontPhotoStorageId);
        Assert.NotEqual(Guid.Empty, actual.BackPhotoStorageId);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(_photoBytes, actual.FrontPhotoBytes);
        Assert.Equal(_photoBytes, actual.BackPhotoBytes);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfDrivingLicenseIdIsEmpty()
    {
        // Arrange

        // Act
        void Act() => Photo.Create(Guid.Empty, _photoBytes, _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(null)]
    public void ThrowValueIsRequiredExceptionIfFrontPhotoBytesIsNullOrEmpty(byte[] invalidFrontPhotoBytes)
    {
        // Arrange

        // Act
        void Act() => Photo.Create(_drivingLicenseId, invalidFrontPhotoBytes, 
            _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(null)]
    public void ThrowValueIsRequiredExceptionIfBackPhotoBytesIsNullOrEmpty(byte[] invalidBackPhotoBytes)
    {
        // Arrange

        // Act
        void Act() => Photo.Create(_drivingLicenseId, _photoBytes, 
            invalidBackPhotoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
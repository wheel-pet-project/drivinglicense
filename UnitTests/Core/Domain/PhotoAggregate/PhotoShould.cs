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

    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(
        accountId: Guid.NewGuid(), 
        categories: [Category.BCategory],
        number: DrivingLicenseNumber.Create(input: "1234 567891"), 
        name: Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), 
        cityOfBirth: "Москва",
        dateOfBirth: new DateOnly(year: 1990, month: 1, day: 1), 
        dateOfIssue: new DateOnly(year: 2020, month: 1, day: 1), 
        codeOfIssue: CodeOfIssue.Create(input: "1234"), 
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1));
    
    [Fact]
    public void CanCreateInstanceWithCorrectValues()
    {
        // Arrange
        var frontPhotoStorageId = Guid.NewGuid();
        var backPhotoStorageId = Guid.NewGuid();

        // Act
        var actual = Photo.Create(_drivingLicense, frontPhotoStorageId, backPhotoStorageId, _photoBytes, _photoBytes);

        // Assert
        Assert.Equal(frontPhotoStorageId, actual.FrontPhotoStorageId);
        Assert.Equal(backPhotoStorageId, actual.BackPhotoStorageId);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(_photoBytes, actual.FrontPhotoBytes);
        Assert.Equal(_photoBytes, actual.BackPhotoBytes);
    }

    [Fact]
    public void CanThrowValueIsRequiredExceptionIfDrivingLicenseIsNull()
    {
        // Arrange
        var frontPhotoStorageId = Guid.NewGuid();
        var backPhotoStorageId = Guid.NewGuid();

        // Act
        void Act() => Photo.Create(null, frontPhotoStorageId, backPhotoStorageId, _photoBytes, _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void CanThrowValueIsRequiredExceptionIfFrontPhotoStorageIdIsEmpty()
    {
        // Arrange
        var frontPhotoStorageId = Guid.Empty;
        var backPhotoStorageId = Guid.NewGuid();

        // Act
        void Act() => Photo.Create(_drivingLicense, frontPhotoStorageId, backPhotoStorageId, _photoBytes, _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Fact]
    public void CanThrowValueIsRequiredExceptionIfBackPhotoStorageIdIsEmpty()
    {
        // Arrange
        var frontPhotoStorageId = Guid.NewGuid();
        var backPhotoStorageId = Guid.Empty;

        // Act
        void Act() => Photo.Create(_drivingLicense, frontPhotoStorageId, backPhotoStorageId, _photoBytes, _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(null)]
    public void CanThrowValueIsRequiredExceptionIfFrontPhotoBytesIsNullOrEmpty(byte[] invalidFrontPhotoBytes)
    {
        // Arrange
        var frontPhotoStorageId = Guid.NewGuid();
        var backPhotoStorageId = Guid.NewGuid();

        // Act
        void Act() => Photo.Create(_drivingLicense, frontPhotoStorageId, backPhotoStorageId, invalidFrontPhotoBytes, 
            _photoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(null)]
    public void CanThrowValueIsRequiredExceptionIfBackPhotoBytesIsNullOrEmpty(byte[] invalidBackPhotoBytes)
    {
        // Arrange
        var frontPhotoStorageId = Guid.NewGuid();
        var backPhotoStorageId = Guid.NewGuid();

        // Act
        void Act() => Photo.Create(_drivingLicense, frontPhotoStorageId, backPhotoStorageId, _photoBytes, 
            invalidBackPhotoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
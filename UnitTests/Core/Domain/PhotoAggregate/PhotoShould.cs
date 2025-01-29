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
        categoryList: CategoryList.Create([CategoryList.BCategory]),
        number: DrivingLicenseNumber.Create(input: "1234 567891"), 
        name: Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), 
        cityOfBirth: "Москва",
        dateOfBirth: new DateOnly(year: 1990, month: 1, day: 1), 
        dateOfIssue: new DateOnly(year: 2020, month: 1, day: 1), 
        codeOfIssue: CodeOfIssue.Create(input: "1234"), 
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1));
    
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Photo.Create(_drivingLicense, _photoBytes, _photoBytes);

        // Assert
        Assert.NotEqual(Guid.Empty, actual.FrontPhotoStorageId);
        Assert.NotEqual(Guid.Empty, actual.BackPhotoStorageId);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(_photoBytes, actual.FrontPhotoBytes);
        Assert.Equal(_photoBytes, actual.BackPhotoBytes);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfDrivingLicenseIsNull()
    {
        // Arrange

        // Act
        void Act() => Photo.Create(null, _photoBytes, _photoBytes);

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
        void Act() => Photo.Create(_drivingLicense, invalidFrontPhotoBytes, 
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
        void Act() => Photo.Create(_drivingLicense, _photoBytes, 
            invalidBackPhotoBytes);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
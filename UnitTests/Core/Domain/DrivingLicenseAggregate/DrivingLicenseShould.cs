using Core.Domain.DrivingLicenceAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

public class DrivingLicenseShould
{
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly DrivingLicenseNumber _number =
        DrivingLicenseNumber.Create(input: "1234 567891");
    private readonly Name _name = Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович");
    private readonly CategoryList _categories = CategoryList.Create([CategoryList.BCategory]);
    private readonly string _cityOfBirth = "Москва";
    private readonly DateOnly _dateOfBirth = new(1990, 1, 1);
    private readonly DateOnly _dateOfIssue = new(2020, 1, 1);
    private readonly CodeOfIssue _codeOfIssue = CodeOfIssue.Create(input: "1234");
    private readonly DateOnly _dateOfExpiry = new DateOnly(2030, 1, 1);
    
    
    [Fact]
    public void CanCreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_accountId, actual.AccountId);
        Assert.Equal(_categories, actual.Categories);
        Assert.Equal(_number, actual.Number);
        Assert.Equal(_dateOfBirth, actual.DateOfBirth);
        Assert.Equal(_dateOfExpiry, actual.DateOfExpiry);
        Assert.Equal(_codeOfIssue, actual.CodeOfIssue);
        Assert.Equal(_dateOfIssue, actual.DateOfIssue);
        Assert.Equal(_dateOfIssue, actual.DateOfIssue);
        Assert.Equal(_codeOfIssue, actual.CodeOfIssue);
        Assert.Equal(_dateOfIssue, actual.DateOfIssue);
    }

    [Fact]
    public void CanThrowValueIsRequiredExceptionIfAccountIdIsEmpty()
    {
        // Arrange
        var emptyAccountId = Guid.Empty;

        // Act
        void Act() => DrivingLicense.Create(emptyAccountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Fact]
    public void ThrowValueIsRequiredExceptionIfCategoriesIsNull()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, null, _number, _name, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfNumberIsNull()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, null, _name, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfNameIsNull()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, null, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ThrowValueIsRequiredExceptionIfCityOfBirthIsNullOrEmpty(string invalidCityOfBirth)
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, invalidCityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfDateOfBirthIsDefault()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, default, 
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Fact]
    public void ThrowValueIsRequiredExceptionIfDateOfIssueIsDefault()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth, 
            default, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfCodeOfIssueIsNull()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, null, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfDateOfExpiryIsDefault()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth, 
            _dateOfIssue, _codeOfIssue, default);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}
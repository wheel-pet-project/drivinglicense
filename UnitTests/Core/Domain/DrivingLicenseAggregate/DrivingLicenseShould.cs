using Domain.DrivingLicenceAggregate;
using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.DrivingLicenseAggregate;

[TestSubject(typeof(DrivingLicense))]
public class DrivingLicenseShould
{
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly DrivingLicenseNumber _number =
        DrivingLicenseNumber.Create(input: "1234 567891");
    private readonly Name _name = Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович");
    private readonly CategoryList _categories = CategoryList.Create([CategoryList.BCategory]);
    private readonly City _cityOfBirth = City.Create("Москва");
    private readonly DateOnly _dateOfBirth = new(1990, 1, 1);
    private readonly DateOnly _dateOfIssue = new(2020, 1, 1);
    private readonly CodeOfIssue _codeOfIssue = CodeOfIssue.Create(input: "1234");
    private readonly DateOnly _dateOfExpiry = new DateOnly(2030, 1, 1);
    
    
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_accountId, actual.AccountId);
        Assert.Equal(_categories, actual.CategoryList);
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
    public void ThrowValueIsRequiredExceptionIfAccountIdIsEmpty()
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

    [Fact]
    public void ThrowValueIsRequiredExceptionIfCityOfBirthIsNull()
    {
        // Arrange

        // Act
        void Act() => DrivingLicense.Create(_accountId, _categories, _number, _name, null, _dateOfBirth, 
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

    [Fact]
    public void SetPendingProcessingStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Act
        license.MarkAsPendingProcessing();

        // Assert
        Assert.Equal(Status.PendingProcessing, license.Status);
    }
    
    [Fact]
    public void SetApproveStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        license.MarkAsPendingProcessing();

        // Act
        license.Approve();

        // Assert
        Assert.Equal(Status.Approved, license.Status);
    }

    [Fact]
    public void AddDomainEventIfLicenseApproved()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        license.MarkAsPendingProcessing();

        // Act
        license.Approve();

        // Assert
        Assert.NotNull(license.DomainEvents[0]);
        Assert.IsType<DrivingLicenseApprovedDomainEvent>(license.DomainEvents[0]);
    }

    [Fact]
    public void ThrowDomainRulesViolationExceptionIfApproveInvokeWithInvalidStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        
        // Act
        void Act() => license.Approve();

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }

    [Fact]
    public void SetRejectedStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        license.MarkAsPendingProcessing();

        // Act
        license.Reject();

        // Assert
        Assert.Equal(Status.Rejected, license.Status);
    }
    
    [Fact]
    public void ThrowDomainRulesViolationExceptionIfRejectInvokeWithApprovedLicense()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Act
        void Act() => license.Reject();

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }

    [Fact]
    public void SetExpiredStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        license.MarkAsPendingProcessing();
        license.Approve();

        // Act
        license.Expire();

        // Assert
        Assert.Equal(Status.Expired, license.Status);
    }

    [Fact]
    public void AddDomainEventIfLicenseExpired()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);
        license.MarkAsPendingProcessing();
        license.Approve();

        // Act
        license.Expire();

        // Assert
        Assert.NotNull(license.DomainEvents[1]);
        Assert.IsType<DrivingLicenseExpiredDomainEvent>(license.DomainEvents[1]);
    }
    
    [Fact]
    public void ThrowDomainRulesViolationExceptionIfExpireInvokeWithInvalidStatus()
    {
        // Arrange
        var license = DrivingLicense.Create(_accountId, _categories, _number, _name, _cityOfBirth, _dateOfBirth,
            _dateOfIssue, _codeOfIssue, _dateOfExpiry);

        // Act
        void Act() => license.Expire();

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }
}
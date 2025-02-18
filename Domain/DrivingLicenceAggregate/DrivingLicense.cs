using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Domain.SharedKernel.ValueObjects;

namespace Domain.DrivingLicenceAggregate;

public sealed class DrivingLicense : Aggregate
{
    private DrivingLicense()
    {
    }

    private DrivingLicense(
        Guid accountId,
        CategoryList categoryListList,
        DrivingLicenseNumber number,
        Name name,
        City cityOfBirth,
        DateOnly dateOfBirth,
        DateOnly dateOfIssue,
        CodeOfIssue codeOfIssue,
        DateOnly dateOfExpiry) : this()
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Status = Status.PendingPhotosAdding;
        CategoryList = categoryListList;
        Number = number;
        Name = name;
        CityOfBirth = cityOfBirth;
        DateOfBirth = dateOfBirth;
        DateOfIssue = dateOfIssue;
        CodeOfIssue = codeOfIssue;
        DateOfExpiry = dateOfExpiry;
    }


    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Status Status { get; private set; } = null!;
    public CategoryList CategoryList { get; private set; } = null!;
    public DrivingLicenseNumber Number { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public City CityOfBirth { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public DateOnly DateOfIssue { get; private set; }
    public CodeOfIssue CodeOfIssue { get; private set; } = null!;
    public DateOnly DateOfExpiry { get; private set; }

    public void MarkAsPendingProcessing()
    {
        if (!Status.CanBeChangedToThisStatus(Status.PendingProcessing))
            throw new DomainRulesViolationException($"{nameof(Status.Rejected)} status can't be settled");

        Status = Status.PendingProcessing;
    }

    public void Approve()
    {
        if (!Status.CanBeChangedToThisStatus(Status.Approved))
            throw new DomainRulesViolationException($"{nameof(Status.Approved)} status can't be settled");

        Status = Status.Approved;
        AddDomainEvent(new DrivingLicenseApprovedDomainEvent(AccountId, [..CategoryList.Categories]));
    }

    public void Reject()
    {
        if (!Status.CanBeChangedToThisStatus(Status.Rejected))
            throw new DomainRulesViolationException($"{nameof(Status.Rejected)} status can't be settled");

        Status = Status.Rejected;
    }

    public void Expire(TimeProvider timeProvider)
    {
        if (timeProvider == null) throw new ValueIsRequiredException($"{nameof(timeProvider)} can't be null");
        
        if (!Status.CanBeChangedToThisStatus(Status.Expired))
            throw new DomainRulesViolationException($"{nameof(Status.Expired)} status can't be settled");
        if (DateOfExpiry > DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            throw new DomainRulesViolationException($"Date of expiry hasn't come yet");
        
        Status = Status.Expired;
        AddDomainEvent(new DrivingLicenseExpiredDomainEvent(AccountId));
    }

    public static DrivingLicense Create(
        Guid accountId,
        CategoryList categoryList,
        DrivingLicenseNumber number,
        Name name,
        City cityOfBirth,
        DateOnly dateOfBirth,
        DateOnly dateOfIssue,
        CodeOfIssue codeOfIssue,
        DateOnly dateOfExpiry,
        TimeProvider timeProvider)
    {
        if (accountId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (categoryList == null)
            throw new ValueIsRequiredException($"{nameof(categoryList)} cannot be null");
        if (number is null)
            throw new ValueIsRequiredException($"{nameof(number)} cannot be null");
        if (name is null)
            throw new ValueIsRequiredException($"{nameof(name)} cannot be  null");
        if (cityOfBirth is null)
            throw new ValueIsRequiredException($"{nameof(cityOfBirth)} cannot null");
        if (dateOfBirth == default)
            throw new ValueIsRequiredException($"{nameof(dateOfBirth)} cannot be default value");
        if (dateOfIssue == default)
            throw new ValueIsRequiredException($"{nameof(dateOfIssue)} cannot be default value");
        if (codeOfIssue is null)
            throw new ValueIsRequiredException($"{nameof(codeOfIssue)} cannot be null");
        if (dateOfExpiry == default)
            throw new ValueIsRequiredException($"{nameof(dateOfExpiry)} cannot be default value");
        if (timeProvider == null)
            throw new ValueIsRequiredException($"{nameof(timeProvider)} cannot be null");

        if (dateOfBirth > dateOfIssue || dateOfExpiry <= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            throw new ValueOutOfRangeException("invalid date(-s) in driving licence");

        return new DrivingLicense(accountId, categoryList, number, name, cityOfBirth, dateOfBirth, dateOfIssue,
            codeOfIssue, dateOfExpiry);
    }
}
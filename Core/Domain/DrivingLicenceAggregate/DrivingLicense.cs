using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.DrivingLicenceAggregate;

public sealed class DrivingLicense : Aggregate
{
    private DrivingLicense(){}

    private DrivingLicense(
        Guid accountId,
        CategoryList categoryList,
        DrivingLicenseNumber number,
        Name name,
        string cityOfBirth,
        DateOnly dateOfBirth,
        DateOnly dateOfIssue,
        CodeOfIssue codeOfIssue,
        DateOnly dateOfExpiry) : this()
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Status = Status.Unprocessed;
        Categories = categoryList;
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

    public CategoryList Categories { get; private set; } = null!;

    public DrivingLicenseNumber Number { get; private set; } = null!;
    
    public Name Name { get; private set; } = null!;
    
    public string CityOfBirth { get; private set; } = null!;

    public DateOnly DateOfBirth { get; private set; }
    
    public DateOnly DateOfIssue { get; private set; }
    
    public CodeOfIssue CodeOfIssue { get; private set; } = null!;
    
    public DateOnly DateOfExpiry { get; private set; }

    public static DrivingLicense Create(
        Guid accountId,
        CategoryList categoryList,
        DrivingLicenseNumber number,
        Name name,
        string cityOfBirth,
        DateOnly dateOfBirth,
        DateOnly dateOfIssue,
        CodeOfIssue codeOfIssue,
        DateOnly dateOfExpiry)
    {
        if (accountId == Guid.Empty) 
            throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (categoryList == null) 
            throw new ValueIsRequiredException($"{nameof(categoryList)} cannot be null");
        if (number is null) 
            throw new ValueIsRequiredException($"{nameof(number)} cannot be null");
        if (name is null) 
            throw new ValueIsRequiredException($"{nameof(name)} cannot be  null");
        if (string.IsNullOrWhiteSpace(cityOfBirth)) 
            throw new ValueIsRequiredException($"{nameof(cityOfBirth)} cannot null");
        if (dateOfBirth == default) 
            throw new ValueIsRequiredException($"{nameof(dateOfBirth)} cannot be default value");
        if (dateOfIssue == default) 
            throw new ValueIsRequiredException($"{nameof(dateOfIssue)} cannot be default value");
        if (codeOfIssue is null) 
            throw new ValueIsRequiredException($"{nameof(codeOfIssue)} cannot be null");
        if (dateOfExpiry == default) 
            throw new ValueIsRequiredException($"{nameof(dateOfExpiry)} cannot be default value");

        if (dateOfBirth > dateOfIssue || dateOfExpiry < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValueOutOfRangeException("invalid date(-s) in driving licence");

        return new DrivingLicense(accountId, categoryList, number, name, cityOfBirth, dateOfBirth, dateOfIssue, 
            codeOfIssue,
            dateOfExpiry);
    }
}
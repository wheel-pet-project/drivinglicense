using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicExceptions;

namespace Domain.SharedKernel.ValueObjects;

public class DrivingLicenseNumber : ValueObject
{
    private DrivingLicenseNumber()
    {
    }

    private DrivingLicenseNumber(string value) : this()
    {
        Value = value;
    }


    public string Value { get; } = null!;

    public static DrivingLicenseNumber Create(string input)
    {
        const int licenseNumberLength = 10;
        
        if (string.IsNullOrWhiteSpace(input))
            throw new ValueIsRequiredException("Driving license number cannot be null or empty");

        var number = input.Trim().Replace(" ", "");

        if (number.Length != licenseNumberLength) throw new ValueIsUnsupportedException(
            "Driving license number must be 10 digits");

        if (!number.All(char.IsDigit))
            throw new ValueIsInvalidException("Driving license number must be a number");

        return new DrivingLicenseNumber(number);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
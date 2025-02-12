using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.SharedKernel.ValueObjects;

public class CodeOfIssue : ValueObject
{
    private CodeOfIssue()
    {
    }

    private CodeOfIssue(string value) : this()
    {
        Value = value;
    }


    public string Value { get; private set; } = null!;

    public static CodeOfIssue Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ValueIsRequiredException("Code of issue cannot be null or empty");

        var code = input.Trim();

        if (code.Length != 4) throw new ValueOutOfRangeException("Code of issue must be 4 digits");

        if (!code.All(char.IsDigit))
            throw new ValueIsInvalidException("Code of issue must be a number");

        return new CodeOfIssue(code);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
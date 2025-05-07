using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicExceptions;

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


    public string Value { get; } = null!;

    public static CodeOfIssue Create(string input)
    {
        const int codeOfIssueLength = 4;
        
        if (string.IsNullOrWhiteSpace(input))
            throw new ValueIsRequiredException("Code of issue cannot be null or empty");

        var code = input.Trim();

        if (code.Length != codeOfIssueLength) throw new ValueIsUnsupportedException("Code of issue must be 4 digits");

        if (!code.All(char.IsDigit))
            throw new ValueIsInvalidException("Code of issue must be a number");

        return new CodeOfIssue(code);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
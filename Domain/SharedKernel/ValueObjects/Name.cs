using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.SharedKernel.ValueObjects;

public class Name : ValueObject
{
    private Name(){}

    private Name(string firstName, string lastName, string? patronymic = null) : this()
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }
    
    
    public string FirstName { get; private set; } = null!;
    
    public string LastName { get; private set; } = null!;
    
    public string? Patronymic { get; private set; }

    public static Name Create(string firstName, string lastName, string? patronymic = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValueIsRequiredException($"{nameof(firstName)} cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ValueIsRequiredException($"{nameof(lastName)} cannot be empty");
        if (patronymic is not null && string.IsNullOrWhiteSpace(patronymic))
            throw new ValueIsRequiredException($"{nameof(patronymic)} cannot be empty if not null");
        
        return new Name(firstName, lastName, patronymic);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        if (Patronymic is not null) yield return Patronymic;
    }
}
using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicExceptions;

namespace Domain.SharedKernel.ValueObjects;

public class City : ValueObject
{
    private City()
    {
    }

    public City(string name)
    {
        Name = name;
    }


    public string Name { get; }

    public static City Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ValueIsRequiredException("city name cannot be empty");

        var cityName = input.Trim();

        cityName = char.ToUpper(cityName[0]) + cityName[1..];

        return new City(cityName);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using CSharpFunctionalExtensions;

namespace Core.Domain.DrivingLicenceAggregate;

public sealed class Status : Entity<int>
{
    public static readonly Status Unprocessed = new(1, nameof(Unprocessed).ToLowerInvariant());
    public static readonly Status Approved = new(2, nameof(Approved).ToLowerInvariant());
    public static readonly Status Declined = new(3, nameof(Declined).ToLowerInvariant());
    
    private Status(){}

    private Status(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }
    
    
    public string Name { get; private set; }
    
    public static IEnumerable<Status> All() =>
    [
        Unprocessed, 
        Approved, 
        Declined
    ];

    public static Status FromName(string name)
    {
        var status = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (status == null) throw new ValueOutOfRangeException($"{nameof(name)} unknown status or null");
        return status;
    }

    public static Status FromId(int id)
    {
        var status = All().SingleOrDefault(s => s.Id == id);
        if (status == null) throw new ValueOutOfRangeException($"{nameof(id)} unknown status or null");
        return status;
    }

    public static bool operator == (Status? a, Status? b)
    {
        if (a is null && b is null)
            return true;
        
        if (a is null || b is null)
            return false;
        
        return a.Id == b.Id;
    }

    public static bool operator != (Status a, Status b) => !(a == b);
}
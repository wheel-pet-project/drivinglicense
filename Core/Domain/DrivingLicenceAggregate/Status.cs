using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using CSharpFunctionalExtensions;

namespace Core.Domain.DrivingLicenceAggregate;

public sealed class Status : Entity<int>
{
    public static readonly Status PendingPhotosAdding = new(1, nameof(PendingPhotosAdding).ToLowerInvariant());
    public static readonly Status PendingProcessing = new(2, nameof(PendingProcessing).ToLowerInvariant());
    public static readonly Status Approved = new(3, nameof(Approved).ToLowerInvariant());
    public static readonly Status Rejected = new(4, nameof(Rejected).ToLowerInvariant());
    public static readonly Status Expired = new(5, nameof(Expired).ToLowerInvariant());
    
    private Status(){}

    private Status(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }


    public string Name { get; private set; } = null!;
    
    public static IEnumerable<Status> All() =>
    [
        PendingPhotosAdding,
        PendingProcessing, 
        Approved, 
        Rejected,
        Expired
    ];

    public bool CanBeChangedToThisStatus(Status potentialStatus)
    {
        if (potentialStatus is null) throw new ValueIsRequiredException($"{nameof(potentialStatus)} cannot be null");
        if (!All().Contains(potentialStatus))
            throw new ValueOutOfRangeException($"{nameof(potentialStatus)} cannot be unsupported");
        
        return potentialStatus switch
        {
            _ when this == potentialStatus => false,
            _ when this == PendingPhotosAdding && potentialStatus == PendingProcessing => true,
            _ when this == PendingProcessing && (potentialStatus == Approved || potentialStatus == Rejected) => true,
            _ when this == Approved && potentialStatus == Expired => true,
            _ => false
        };
    }

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
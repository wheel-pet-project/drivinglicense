using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.InternalExceptions;
using Domain.SharedKernel.Exceptions.PublicExceptions;

namespace Domain.DrivingLicenceAggregate;

public sealed class Status : Entity<int>
{
    public static readonly Status PendingPhotosAdding = new(1, nameof(PendingPhotosAdding).ToLowerInvariant());
    public static readonly Status PendingProcessing = new(2, nameof(PendingProcessing).ToLowerInvariant());
    public static readonly Status Approved = new(3, nameof(Approved).ToLowerInvariant());
    public static readonly Status Rejected = new(4, nameof(Rejected).ToLowerInvariant());
    public static readonly Status Expired = new(5, nameof(Expired).ToLowerInvariant());

    private Status()
    {
    }

    private Status(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }


    public string Name { get; } = null!;

    public bool CanBeChangedToThisStatus(Status potentialStatus)
    {
        if (potentialStatus is null) throw new ValueIsRequiredException($"{nameof(potentialStatus)} cannot be null");
        if (!All().Contains(potentialStatus))
            throw new ValueIsUnsupportedException($"{nameof(potentialStatus)} cannot be unsupported");

        return potentialStatus switch
        {
            _ when this == potentialStatus => throw new AlreadyHaveThisStateException(
                "License already have this status"),
            _ when this == PendingPhotosAdding && potentialStatus == PendingProcessing => true,
            _ when this == PendingProcessing && (potentialStatus == Approved || potentialStatus == Rejected) => true,
            _ when this == Approved && potentialStatus == Expired => true,
            _ => false
        };
    }

    public static IEnumerable<Status> All()
    {
        return
        [
            PendingPhotosAdding,
            PendingProcessing,
            Approved,
            Rejected,
            Expired
        ];
    }

    public static Status FromName(string name)
    {
        var status = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (status == null) throw new ValueIsUnsupportedException($"{nameof(name)} unknown status or null");
        return status;
    }

    public static Status FromId(int id)
    {
        var status = All().SingleOrDefault(s => s.Id == id);
        if (status == null) throw new ValueIsUnsupportedException($"{nameof(id)} unknown status or null");
        return status;
    }

    public static bool operator ==(Status? a, Status? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Id == b.Id;
    }

    public static bool operator !=(Status a, Status b)
    {
        return !(a == b);
    }
}
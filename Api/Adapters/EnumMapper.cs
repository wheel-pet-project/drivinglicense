using Domain.SharedKernel.Exceptions.ArgumentException;
using Proto.DrivingLicenseV1;
using DomainStatus = Domain.DrivingLicenceAggregate.Status;

namespace Api.Adapters;

public class EnumMapper
{
    public DomainStatus ProtoStatusToDomainStatus(Status? protoStatus)
    {
        return protoStatus switch
        {
            Status.PendingPhotosAdding => DomainStatus.PendingPhotosAdding,
            Status.PendingProcessing => DomainStatus.PendingProcessing,
            Status.ApprovedUnspecified => DomainStatus.Approved,
            Status.Rejected => DomainStatus.Rejected,
            Status.Expired => DomainStatus.Expired,
            _ => throw new ValueOutOfRangeException($"{nameof(protoStatus)} is unknown status")
        };
    }

    public Status DomainStatusToProtoStatus(DomainStatus domainStatus)
    {
        return domainStatus switch
        {
            _ when domainStatus == DomainStatus.PendingPhotosAdding => Status.PendingPhotosAdding,
            _ when domainStatus == DomainStatus.PendingProcessing => Status.PendingProcessing,
            _ when domainStatus == DomainStatus.Approved => Status.ApprovedUnspecified,
            _ when domainStatus == DomainStatus.Rejected => Status.Rejected,
            _ when domainStatus == DomainStatus.Expired => Status.Expired,
            _ => throw new ValueOutOfRangeException($"{nameof(domainStatus)} is unknown status")
        };
    }
}
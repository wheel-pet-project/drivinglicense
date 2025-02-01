using Domain.DrivingLicenceAggregate;
using MediatR;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public record GetAllDrivingLicensesQuery(
    Guid CorrelationId,
    int Page, 
    int PageSize, 
    Status? FilteringStatus = null) 
    : BaseRequest(CorrelationId), IRequest<GetAllDrivingLicensesQueryResponse>;
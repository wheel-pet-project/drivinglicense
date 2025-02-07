using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public record GetAllDrivingLicensesQuery(
    Guid CorrelationId,
    int Page, 
    int PageSize, 
    Status FilteringStatus) 
    : BaseRequest(CorrelationId), IRequest<Result<GetAllDrivingLicensesQueryResponse>>;
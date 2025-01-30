using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public record GetByIdDrivingLicenseQuery(
    Guid CorrelationId, 
    Guid Id) 
    : BaseRequest(CorrelationId), IRequest<Result<GetByIdDrivingLicenseQueryResponse>>;
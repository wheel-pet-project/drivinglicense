using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public record GetByIdDrivingLicenseQuery(Guid Id) : IRequest<Result<GetByIdDrivingLicenseQueryResponse>>;
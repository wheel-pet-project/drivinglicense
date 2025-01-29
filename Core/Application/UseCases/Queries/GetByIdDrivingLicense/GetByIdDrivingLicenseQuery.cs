using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Queries.GetByIdDrivingLicense;

public record GetByIdDrivingLicenseQuery(Guid Id) : IRequest<Result<GetByIdDrivingLicenseQueryResponse>>;
using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public record GetAllDrivingLicensesQuery(
    int Page,
    int PageSize,
    Status FilteringStatus)
    : IRequest<Result<GetAllDrivingLicensesQueryResponse>>;
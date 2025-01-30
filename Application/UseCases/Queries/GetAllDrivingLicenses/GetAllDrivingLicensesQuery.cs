using Domain.DrivingLicenceAggregate;
using MediatR;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public record GetAllDrivingLicensesQuery(
    int Page, 
    int PageSize, 
    Status? FilterStatus = null) : IRequest<GetAllDrivingLicensesQueryResponse>;
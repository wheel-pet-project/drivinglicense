using Core.Domain.DrivingLicenceAggregate;
using MediatR;

namespace Core.Application.UseCases.Queries.GetAllDrivingLicenses;

public record GetAllDrivingLicensesQuery(
    int Page, 
    int PageSize, 
    Status? FilterStatus = null) : IRequest<GetAllDrivingLicensesQueryResponse>;
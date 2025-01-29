using MediatR;

namespace Core.Application.UseCases.Queries.GetAllDrivingLicenses;

public class GetAllDrivingLicensesQueryHandler : IRequestHandler<GetAllDrivingLicensesQuery, GetAllDrivingLicensesQueryResponse>
{
    public Task<GetAllDrivingLicensesQueryResponse> Handle(GetAllDrivingLicensesQuery request, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
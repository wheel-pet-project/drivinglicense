using Domain.DrivingLicenceAggregate;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public class GetAllDrivingLicensesQueryResponse
{
    private readonly List<DrivingLicenseShortView> _drivingLicenses;

    public GetAllDrivingLicensesQueryResponse(List<DrivingLicenseShortView> drivingLicenses)
    {
        _drivingLicenses = drivingLicenses;
    }

    public IReadOnlyList<DrivingLicenseShortView> DrivingLicenses => _drivingLicenses;

    public record DrivingLicenseShortView(Guid Id, Guid AccountId, string Name, Status Status);
}
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

    public class DrivingLicenseShortView
    {
        public required Guid Id { get; init; }

        public required Guid AccountId { get; init; }

        public required string Name { get; init; } = null!;

        public required Status Status { get; init; } = null!;
    }
}
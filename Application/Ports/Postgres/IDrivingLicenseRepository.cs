using Domain.DrivingLicenceAggregate;

namespace Application.Ports.Postgres;

public interface IDrivingLicenseRepository
{
    Task<DrivingLicense?> GetById(Guid id);

    void Update(DrivingLicense drivingLicense);

    Task Add(DrivingLicense drivingLicense);
}
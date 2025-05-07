using Domain.DrivingLicenceAggregate;

namespace Application.Ports.Postgres;

public interface IDrivingLicenseRepository
{
    Task<DrivingLicense?> GetById(Guid id);

    Task<List<DrivingLicense>> GetAllExpired();

    void Update(DrivingLicense drivingLicense);

    Task Add(DrivingLicense drivingLicense);
}
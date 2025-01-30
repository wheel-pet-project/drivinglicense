using System.Linq.Expressions;
using Domain.DrivingLicenceAggregate;

namespace Application.Ports.Postgres;

public interface IDrivingLicenseRepository
{
    Task<List<DrivingLicense>> GetAll(int page, int pageSize, Expression<Func<DrivingLicense, bool>>? predicate = null);

    Task<DrivingLicense?> GetById(Guid id);

    void Update(DrivingLicense drivingLicense);
    
    Task Add(DrivingLicense drivingLicense);
}
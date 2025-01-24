using System.Linq.Expressions;
using Core.Domain.DrivingLicenceAggregate;

namespace Core.Ports.Postgres;

public interface IDrivingLicenseRepository
{
    Task<List<DrivingLicense>> GetAll(int page, int pageSize, Expression<Func<DrivingLicense, bool>> predicate);

    Task<DrivingLicense?> GetById(Guid id);

    void Update(DrivingLicense drivingLicense);
    
    Task Add(DrivingLicense drivingLicense);
}
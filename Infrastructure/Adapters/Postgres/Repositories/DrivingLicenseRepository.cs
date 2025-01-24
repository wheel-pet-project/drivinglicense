using System.Linq.Expressions;
using Core.Domain.DrivingLicenceAggregate;
using Core.Ports.Postgres;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class DrivingLicenseRepository(DataContext context) : IDrivingLicenseRepository
{
    public async Task<List<DrivingLicense>> GetAll(int page, int pageSize, 
        Expression<Func<DrivingLicense, bool>> predicate)
    {
        return await context.DrivingLicenses
            .Include(x => x.Status)
            .Include(x => x.Categories)
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<DrivingLicense?> GetById(Guid id)
    {
        return await context.DrivingLicenses
            .Include(x => x.Status)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);
        context.Attach(drivingLicense.Categories);
        
        context.Update(drivingLicense);
    }

    public async Task Add(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);
        context.Attach(drivingLicense.Categories);
        
        await context.AddAsync(drivingLicense);
    }
}
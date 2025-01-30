using System.Linq.Expressions;
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class DrivingLicenseRepository(DataContext context) : IDrivingLicenseRepository
{
    public async Task<List<DrivingLicense>> GetAll(int page, int pageSize, 
        Expression<Func<DrivingLicense, bool>>? predicate = null)
    {
        var queryable = context.DrivingLicenses
            .Include(x => x.Status)
            .Include(x => x.CategoryList);

        return await (predicate != null
            ? queryable
                .Where(predicate)
                .Skip(GetSkipCount(page, pageSize))
                .Take(pageSize)
                .ToListAsync()
            : queryable
                .Skip(GetSkipCount(page, pageSize))
                .Take(pageSize)
                .ToListAsync());

        int GetSkipCount(int p, int pSize) => (p - 1) * pSize;
    }

    public async Task<DrivingLicense?> GetById(Guid id)
    {
        return await context.DrivingLicenses
            .Include(x => x.Status)
            .Include(x => x.CategoryList)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);
        context.Attach(drivingLicense.CategoryList);
        
        context.Update(drivingLicense);
    }

    public async Task Add(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);
        context.Attach(drivingLicense.CategoryList);
        
        await context.AddAsync(drivingLicense);
    }
}
using System.Linq.Expressions;
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class DrivingLicenseRepository(DataContext context) : IDrivingLicenseRepository
{ 
    public async Task<DrivingLicense?> GetById(Guid id)
    {
        return await context.DrivingLicenses
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);

        context.Update(drivingLicense);
    }

    public async Task Add(DrivingLicense drivingLicense)
    {
        context.Attach(drivingLicense.Status);

        await context.AddAsync(drivingLicense);
    }
}
using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Adapters.Postgres.ActualityObserver;

/// <summary>
///     Отслеживает истекшие водительские права
/// </summary>
public class ActualityObserverBackgroundJob(
    DataContext context,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ActualityObserverBackgroundJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        var expiredLicenses = context.DrivingLicenses
            .Include(x => x.Status)
            .Where(x => x.DateOfExpiry < DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime.Date) &&
                        x.Status != Status.Expired)
            .OrderBy(x => x.DateOfExpiry)
            .ToList();

        if (expiredLicenses.Count > 0)
            foreach (var license in expiredLicenses)
                try
                {
                    license.Expire(timeProvider);
                    context.Attach(license.Status);
                    context.Update(license);
                }
                catch (Exception e)
                {
                    logger.LogCritical("Fail to update expiry status for license, exception: {e}", e);
                }

        await unitOfWork.Commit();
    }
}
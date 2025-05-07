using Application.Ports.Postgres;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Adapters.Postgres.ActualityObserver;

/// <summary>
///     Отслеживает истекшие водительские права
/// </summary>
public class ActualityObserverBackgroundJob(
    IDrivingLicenseRepository  drivingLicenseRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ActualityObserverBackgroundJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        var expiredLicenses = await drivingLicenseRepository.GetAllExpired();

        if (expiredLicenses.Count > 0)
            foreach (var license in expiredLicenses)
                try
                {
                    license.Expire(timeProvider);
                    
                    drivingLicenseRepository.Update(license);
                }
                catch (Exception e)
                {
                    logger.LogCritical(
                        "Fail to process update license status in domain event handler, exception: {e}", e);
                }
        
        await unitOfWork.Commit();
    }
}
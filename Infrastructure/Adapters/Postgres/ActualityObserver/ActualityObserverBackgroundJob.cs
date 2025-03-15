using Application.Ports.Postgres;
using Dapper;
using Domain.DrivingLicenceAggregate;
using Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace Infrastructure.Adapters.Postgres.ActualityObserver;

/// <summary>
///     Отслеживает истекшие водительские права
/// </summary>
public class ActualityObserverBackgroundJob(
    NpgsqlDataSource dataSource,
    IMediator mediator,
    TimeProvider timeProvider,
    ILogger<ActualityObserverBackgroundJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        List<ExpiredDrivingLicenseDapperModel> expiredLicenses;

        await using (var connection = await dataSource.OpenConnectionAsync())
        {
            expiredLicenses = (await connection.QueryAsync<ExpiredDrivingLicenseDapperModel>(Sql,
                    new { Today = timeProvider.GetUtcNow().DateTime, ExpiredStatusId = Status.Expired.Id }))
                .AsList();
        }

        if (expiredLicenses.Count > 0)
            foreach (var license in expiredLicenses)
                try
                {
                    await mediator.Publish(new DrivingLicenseExpiredDomainEvent(license.Id, license.AccountId));
                }
                catch (Exception e)
                {
                    logger.LogCritical(
                        "Fail to process update osago expiry status in domain event handler, exception: {e}", e);
                }
    }

    private record ExpiredDrivingLicenseDapperModel(Guid Id, Guid AccountId);

    private const string Sql =
        """
        SELECT id AS Id,
               account_id AS AccountId
        FROM driving_license
        WHERE date_of_expiry < @Today AND 
              status_id != @ExpiredStatusId
        ORDER BY date_of_expiry
        LIMIT 100
        """;
}
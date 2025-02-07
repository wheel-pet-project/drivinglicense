using Dapper;
using Domain.DrivingLicenceAggregate.DomainEvents;
using MediatR;
using Npgsql;
using Quartz;

namespace Infrastructure.Adapters.Postgres.ActualityObserver;

/// <summary>
/// Отслеживает истекшие водительские права
/// </summary>
public class ActualityObserverBackgroundJob(
    NpgsqlDataSource dataSource,
    TimeProvider timeProvider,
    IMediator mediator) 
    : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var command = new CommandDefinition(_sql, new { Today = timeProvider.GetUtcNow().UtcDateTime });
        
        var licenseIdEnumerable = await connection.QueryAsync<Guid>(command);
        var licenseIdList = licenseIdEnumerable.AsList();
        
        if (licenseIdList.Count > 0)
            foreach (var licenseId in licenseIdList)
                await mediator.Publish(new DrivingLicenseExpiredDomainEvent(licenseId),
                    jobExecutionContext.CancellationToken);
    }

    private readonly string _sql =
        """
        SELECT id AS Id
        FROM driving_license
        WHERE date_of_expiry <= '@Today'
        """;
}
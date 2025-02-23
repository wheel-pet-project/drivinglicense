using Domain.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Outbox;

[DisallowConcurrentExecution]
public class OutboxBackgroundJob(
    DataContext context,
    IMediator mediator,
    ILogger<OutboxBackgroundJob> logger)
    : IJob
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        var outboxEvents = await context.Outbox
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(50)
            .ToListAsync();

        if (outboxEvents.Count > 0)
        {
            var domainEvents = outboxEvents
                .Select(e => JsonConvert.DeserializeObject<DomainEvent>(e.Content, _jsonSerializerSettings))
                .OfType<DomainEvent>()
                .ToList()
                .AsReadOnly();

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                for (var i = 0; i < domainEvents.Count; i++)
                {
                    await mediator.Publish(domainEvents[i], jobExecutionContext.CancellationToken);
                    outboxEvents[i].MarkProcessed();
                }

                context.Outbox.UpdateRange(outboxEvents);
                await context.SaveChangesAsync(jobExecutionContext.CancellationToken);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                logger.LogError("Failed of processing outbox events and save updates, exception: {e}", e);
                await transaction.RollbackAsync();
            }
        }
    }
}
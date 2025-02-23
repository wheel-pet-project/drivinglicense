using Application.Ports.Postgres;
using Domain.SharedKernel;
using Domain.SharedKernel.Errors;
using FluentResults;
using Infrastructure.Adapters.Postgres.Outbox;
using Newtonsoft.Json;

namespace Infrastructure.Adapters.Postgres;

public sealed class UnitOfWork(DataContext context) : IUnitOfWork, IDisposable
{
    public async Task<Result> Commit()
    {
        await SaveDomainEventsInOutbox();

        try
        {
            await context.SaveChangesAsync();
            return Result.Ok();
        }
        catch
        {
            return Result.Fail(new CommitFail("Failed to commit changes"));
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }

    private async Task SaveDomainEventsInOutbox()
    {
        var outboxEvents = context.ChangeTracker
            .Entries<IAggregate>()
            .Select(x => x.Entity)
            .SelectMany(aggregate =>
            {
                var domainEvents = new List<DomainEvent>(aggregate.DomainEvents);

                aggregate.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxEvent
            {
                EventId = domainEvent.EventId,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(domainEvent,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
            })
            .ToList();

        await context.Outbox.AddRangeAsync(outboxEvents);
    }
}
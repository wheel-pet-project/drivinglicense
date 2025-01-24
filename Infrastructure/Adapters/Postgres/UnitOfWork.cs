using Core.Domain.SharedKernel;
using Core.Ports.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Newtonsoft.Json;

namespace Infrastructure.Adapters.Postgres;

public class UnitOfWork(DataContext context) : IUnitOfWork, IDisposable
{
    public async Task<bool> Commit()
    {
        await SaveDomainEventsInOutbox();
        
        await context.SaveChangesAsync();
        return true;
    }
    
    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
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
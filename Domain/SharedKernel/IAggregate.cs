namespace Domain.SharedKernel;

public interface IAggregate
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }

    public void ClearDomainEvents();
}
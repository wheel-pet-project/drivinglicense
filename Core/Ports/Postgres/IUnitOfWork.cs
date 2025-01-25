namespace Core.Ports.Postgres;

public interface IUnitOfWork
{
    Task<bool> Commit();
}
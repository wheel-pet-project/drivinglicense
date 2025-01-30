namespace Application.Ports.Postgres;

public interface IUnitOfWork
{
    Task<bool> Commit();
}
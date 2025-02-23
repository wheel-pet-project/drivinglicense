using FluentResults;

namespace Application.Ports.Postgres;

public interface IUnitOfWork
{
    Task<Result> Commit();
}
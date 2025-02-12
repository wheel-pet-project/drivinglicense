using Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("notification")
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    protected DataContext Context = null!;
    protected NpgsqlDataSource DataSource = null!;


    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DataContext>().UseNpgsql(_postgreSqlContainer.GetConnectionString(),
            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(DataContext).Assembly));
        Context = new DataContext(options.Options);

        await Context.Database.MigrateAsync();

        DataSource = new NpgsqlDataSourceBuilder(_postgreSqlContainer.GetConnectionString()).Build();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
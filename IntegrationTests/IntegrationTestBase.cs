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
        .WithDatabase("vehiclefleet")
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    protected DataContext Context = null!;
    protected NpgsqlDataSource DataSource = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();
        var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString) { IncludeErrorDetail = true };
        var options = new DbContextOptionsBuilder<DataContext>().UseNpgsql(connectionBuilder.ConnectionString,
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
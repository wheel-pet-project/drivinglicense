using System.Data.Common;
using Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresDataContext(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddTransient<DbDataSource, NpgsqlDataSource>(_ =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Driving_license#" + Environment.MachineName,
                    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                    Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5430"),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "drivinglicense_db",
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password"
                }
            };

            return dataSourceBuilder.Build();
        });

        var serviceProvider = services.BuildServiceProvider();
        var dataSource = serviceProvider.GetRequiredService<DbDataSource>();
        
        services.AddDbContext<DataContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(dataSource,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(DataContext).Assembly));
            optionsBuilder.EnableSensitiveDataLogging();
        });
        
        return services;
    }
}
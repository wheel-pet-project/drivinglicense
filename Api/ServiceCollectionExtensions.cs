using Infrastructure.Adapters.Postgres;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresDataContext(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("DbConnectionSettings").Get<DbConnectionSettings>();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder
        {
            ConnectionStringBuilder =
            {
                ApplicationName = "Notification",
                Host = dbSettings!.Host,
                Port = dbSettings.Port,
                Database = dbSettings.Database,
                Username = dbSettings.Username,
                Password = dbSettings.Password
            }
        };
        var dataSource = dataSourceBuilder.Build(); 
        
        services.AddDbContext<DataContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(dataSource,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(DataContext).Assembly));
            optionsBuilder.EnableSensitiveDataLogging();
        });
        
        return services;
    }
}
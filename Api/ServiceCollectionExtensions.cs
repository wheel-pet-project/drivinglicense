using System.Data.Common;
using Api.Adapters.Mapper;
using Api.PipelineBehaviours;
using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Application.UseCases.Commands.UploadDrivingLicense;
using Infrastructure.Adapters.Kafka;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresDataContext(this IServiceCollection services)
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
    
    public static IServiceCollection RegisterMediatrAndPipelines(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UploadDrivingLicenseHandler).Assembly))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehaviour<,>));
        
        return services;
    }

    public static IServiceCollection RegisterMapper(this IServiceCollection services)
    {
        services.AddScoped<Mapper>();
        
        return services;
    }
    
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddTransient<IDrivingLicenseRepository, DrivingLicenseRepository>();
        services.AddTransient<IPhotoRepository, PhotoRepository>();
    
        return services;
    }
    
    public static IServiceCollection RegisterSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .WriteTo.MongoDBBson(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!,
                "logs",
                LogEventLevel.Verbose,
                50,
                TimeSpan.FromSeconds(10))
            .CreateLogger();
        services.AddSerilog();

        return services;
    }
    
    public static IServiceCollection RegisterMessageBus(this IServiceCollection services)
    {
        services.AddTransient<IMessageBus, KafkaProducer>();
        
        return services;
    }
    
    public static IServiceCollection RegisterTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddPrometheusExporter();

                builder.AddMeter("Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel");
                builder.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries =
                        [
                            0, 0.005, 0.01, 0.025, 0.05,
                            0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                        ]
                    });
            })
            .WithTracing(builder =>
            {
                builder
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddNpgsql()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("DrivingLicense"))
                    .AddSource("DrivingLicense")
                    .AddSource("MassTransit") 
                    .AddJaegerExporter();
            });

        return services;
    }
    
    public static IServiceCollection RegisterHealthCheckV1(this IServiceCollection services)
    {
        var getConnectionString = () =>
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = "Identity" + Environment.MachineName,
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")!),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DB")!,
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!,
                BrowsableConnectionString = false,
            };
            
            return connectionBuilder.ConnectionString;
        };
        
        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg => 
                    cfg.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")!.Split("__")[0], 
                timeout: TimeSpan.FromSeconds(10));
        
        return services;
    }
}
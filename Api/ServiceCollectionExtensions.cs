using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Api.Adapters.Mapper;
using Application.DomainEventHandlers;
using Application.Ports.ImageValidators;
using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Application.UseCases.Commands.ApproveDrivingLicense;
using Application.UseCases.Commands.RejectDrivingLicense;
using Application.UseCases.Commands.UploadDrivingLicense;
using Application.UseCases.Commands.UploadPhotos;
using Application.UseCases.Queries.GetAllDrivingLicenses;
using Application.UseCases.Queries.GetByIdDrivingLicense;
using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.PhotoAggregate.DomainEvents;
using FluentResults;
using From.DrivingLicenseKafkaEvents;
using Infrastructure.Adapters.ImageValidators;
using Infrastructure.Adapters.Kafka;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.ActualityObserver;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Adapters.S3;
using Infrastructure.Options;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresContextAndDataSource(this IServiceCollection services)
    {
        services.AddScoped<NpgsqlDataSource>(_ =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Driving_license#" + Environment.MachineName ?? "dev",
                    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                    Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5430"),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "drivinglicense_db",
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password",
                    BrowsableConnectionString = false
                }
            };

            return dataSourceBuilder.Build();
        });

        var serviceProvider = services.BuildServiceProvider();
        var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();

        services.AddDbContext<DataContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(dataSource,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(DataContext).Assembly));
            optionsBuilder.EnableSensitiveDataLogging();
        });

        return services;
    }

    public static IServiceCollection RegisterUnitOfWork(this IServiceCollection services)
    {
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection RegisterMediatrAndPipelines(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Commands
        services.AddTransient<IRequestHandler<ApproveDrivingLicenseCommand, Result>, ApproveDrivingLicenseHandler>();
        services.AddTransient<IRequestHandler<RejectDrivingLicenseCommand, Result>, RejectDrivingLicenseHandler>();
        services.AddTransient<IRequestHandler<UploadDrivingLicenseCommand, Result<UploadDrivingLicenseResponse>>,
            UploadDrivingLicenseHandler>();
        services.AddTransient<IRequestHandler<UploadPhotosCommand, Result>, UploadPhotosHandler>();

        // Queries
        var serviceProvider = services.BuildServiceProvider();
        services.AddTransient<IRequestHandler<GetByIdDrivingLicenseQuery, Result<GetByIdDrivingLicenseQueryResponse>>>(
            _ => new GetByIdDrivingLicenseQueryHandler(serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                Environment.GetEnvironmentVariable("AWS_S3_SERVICE_URL") ?? "https://storage.yandexcloud.net"));
        services.AddTransient<IRequestHandler<GetAllDrivingLicensesQuery, Result<GetAllDrivingLicensesQueryResponse>>,
            GetAllDrivingLicensesQueryHandler>();

        // Domain event handlers
        services.AddTransient<INotificationHandler<PhotosAddedDomainEvent>, PhotoAddedHandler>();
        services.AddTransient<INotificationHandler<DrivingLicenseApprovedDomainEvent>, DrivingLicenseApprovedHandler>();
        services.AddTransient<INotificationHandler<DrivingLicenseExpiredDomainEvent>, DrivingLicenseExpiredHandler>();

        return services;
    }

    public static IServiceCollection RegisterS3Storage(this IServiceCollection services)
    {
        services.AddTransient<IAmazonS3>(_ =>
            new AmazonS3Client(
                Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "aws_access_key_id",
                Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "aws_secret_access_key",
                new AmazonS3Config
                {
                    ForcePathStyle = true,
                    ServiceURL = Environment.GetEnvironmentVariable("AWS_S3_SERVICE_URL") ??
                                 "https://storage.yandexcloud.net",
                    AuthenticationRegion = "ru-central1",
                    RetryMode = RequestRetryMode.Standard
                }));
        services.AddTransient<IS3Storage, S3Storage>();

        services.Configure<S3Options>(options =>
            options.Buckets = (Environment.GetEnvironmentVariable("AWS_S3_BUCKETS") ?? "default_bucket").Split("__"));

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
            .WriteTo.MongoDBBson(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
                                 ?? "mongodb://carsharing:password@localhost:27017/drivinglicense?authSource=admin",
                "logs",
                LogEventLevel.Verbose,
                50,
                TimeSpan.FromSeconds(10))
            .CreateLogger();
        services.AddSerilog();

        return services;
    }

    public static IServiceCollection RegisterMassTransit(this IServiceCollection services)
    {
        services.AddTransient<IMessageBus, KafkaProducer>();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(rider =>
            {
                rider.AddProducer<string, DrivingLicenseApproved>("driving-license-approved-topic");
                rider.AddProducer<string, DrivingLicenseExpired>("driving-license-expired-topic");

                rider.UsingKafka((_, k) =>
                    k.Host((Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS") ?? "localhost:9092").Split("__")));
            });
        });

        return services;
    }

    public static IServiceCollection RegisterImageValidators(this IServiceCollection services)
    {
        services.AddTransient<IImageFormatValidator, ImageFormatValidator>();
        services.AddTransient<IImageSizeValidator, ImageSizeValidator>();

        return services;
    }

    public static IServiceCollection RegisterOutboxAndActualityObserverBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            var outboxJobKey = new JobKey(nameof(OutboxBackgroundJob));
            configure
                .AddJob<OutboxBackgroundJob>(j => j.WithIdentity(outboxJobKey))
                .AddTrigger(trigger => trigger.ForJob(outboxJobKey)
                    .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));

            var actualityObserverJobKey = new JobKey(nameof(ActualityObserverBackgroundJob));
            configure
                .AddJob<ActualityObserverBackgroundJob>(j => j.WithIdentity(actualityObserverJobKey))
                .AddTrigger(trigger => trigger.ForJob(actualityObserverJobKey)
                    .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection RegisterTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

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
                    .AddGrpcCoreInstrumentation()
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
                ApplicationName = "Driving_license#" + Environment.MachineName,
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5430"),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "drivinglicense_db",
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password",
                BrowsableConnectionString = false
            };

            return connectionBuilder.ConnectionString;
        };

        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg =>
                    cfg.BootstrapServers = (Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")
                                            ?? "localhost:9092").Split("__")[0],
                timeout: TimeSpan.FromSeconds(10));

        return services;
    }
}
using Api.Adapters.Grpc;
using Api.Interceptors;
using Application.UseCases.Queries.DapperMappingExtensions;
using Dapper;

namespace Api;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionHandlerInterceptor>();
            options.Interceptors.Add<TracingInterceptor>();
            options.Interceptors.Add<LoggingInterceptor>();
        });

        services
            .RegisterPostgresContextAndDataSource()
            .RegisterUnitOfWork()
            .RegisterS3Storage()
            .RegisterOutboxAndActualityObserverBackgroundJobs()
            .RegisterMediatrAndHandlers()
            .RegisterMapper()
            .RegisterRepositories()
            .RegisterSerilog()
            .RegisterMassTransit()
            .RegisterImageValidators()
            .RegisterTelemetry()
            .RegisterHealthCheckV1()
            .RegisterTimeProvider();

        var app = builder.Build();

        app.MapGrpcService<DrivingLicenseV1>();
        app.MapGrpcHealthChecksService();

        RegisterDapperMapping();

        app.Run();
        return;

        void RegisterDapperMapping()
        {
            SqlMapper.AddTypeHandler(new DateOnlyMapper());
        }
    }
}
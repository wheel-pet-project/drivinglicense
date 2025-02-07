
using Api.Adapters.Grpc;

namespace Api;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;
        
        services.AddGrpc(options => options.Interceptors.Add<ExceptionHandlerInterceptor>());

        services
            .RegisterPostgresDataContext()
            .RegisterUnitOfWork()
            .RegisterS3Storage()
            .RegisterOutboxAndActualityObserverBackgroundJobs()
            .RegisterMediatrAndPipelines()
            .RegisterMapper()
            .RegisterRepositories()
            .RegisterSerilog()
            .RegisterMassTransit()
            .RegisterTelemetry()
            .RegisterHealthCheckV1()
            .RegisterTimeProvider();
        
        var app = builder.Build();

        app.MapGrpcService<DrivingLicenseV1>();
        app.MapGrpcHealthChecksService();
        
        app.Run();
    }
}
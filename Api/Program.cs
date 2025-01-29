
namespace Api;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;
        
        services.AddGrpc();

        services.RegisterPostgresDataContext(builder.Configuration);
        
        var app = builder.Build();
        

        app.Run();
    }
}
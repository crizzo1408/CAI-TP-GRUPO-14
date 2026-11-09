using Notifications.API.Data;
using Notifications.API.Extensions;
using Serilog;

namespace Notifications.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddAppLogging();

        try
        {
            builder.Services.AddAppServices();

            var app = builder.Build();
            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();
            app.UseAppMiddleware();
            app.MapAppEndpoints();
            app.Run();
        }
        catch (HostAbortedException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Notifications.API no pudo iniciar");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

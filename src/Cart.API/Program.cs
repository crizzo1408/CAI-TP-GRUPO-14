using Serilog;
using Cart.API.Data;
using Cart.API.Extensions;

namespace Cart.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Serilog: se configura antes que cualquier otra cosa (lo pide la consigna)
        builder.AddAppLogging();

        try
        {
            // 2. Servicios: controllers, cliente HTTP de Products, Swagger, health checks, exception handlers
            builder.Services.AddAppServices();

            var app = builder.Build();

            // 3. Crear las tablas de SQLite
            var initializer = app.Services.GetRequiredService<DatabaseInitializer>();
            initializer.Initialize();

            // 4. Middlewares: Correlation ID, logs de request, manejo de errores, Swagger
            app.UseAppMiddleware();

            // 5. Endpoints: controllers + health checks
            app.MapAppEndpoints();

            app.Run();
        }
        catch (HostAbortedException)
        {
            // La lanza Visual Studio al detener la app en algunas herramientas: no es un error
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Cart.API no pudo iniciar");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

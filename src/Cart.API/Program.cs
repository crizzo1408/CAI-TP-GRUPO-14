using Cart.API.Data;
using Cart.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog: se configura antes que cualquier otra cosa (lo pide la consigna)
builder.AddAppLogging();

try
{
    // 2. Servicios: controllers, cliente HTTP de Products, Swagger, health checks, exception handlers
    builder.Services.AddAppServices(builder.Configuration);

    var app = builder.Build();

    // 3. Crear las tablas de SQLite
    app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

    // 4. Middlewares: Correlation ID, logs de request, manejo de errores, Swagger
    app.UseAppMiddleware();

    // 5. Endpoints: controllers + health checks
    app.MapAppEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Cart.API no pudo iniciar");
}
finally
{
    Log.CloseAndFlush();
}

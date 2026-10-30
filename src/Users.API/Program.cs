using Serilog;
using Users.API.Data;
using Users.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog: se configura antes que cualquier otra cosa (lo pide la consigna)
builder.AddAppLogging();

try
{
    // 2. Servicios: controllers, Swagger, health checks, exception handlers, repositorios
    builder.Services.AddAppServices(builder.Configuration);

    var app = builder.Build();

    // 3. Crear las tablas de SQLite (y el usuario de prueba en Development)
    app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

    // 4. Middlewares: Correlation ID, logs de request, manejo de errores, Swagger
    app.UseAppMiddleware();

    // 5. Endpoints: controllers + health checks
    app.MapAppEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Users.API no pudo iniciar");
}
finally
{
    Log.CloseAndFlush();
}

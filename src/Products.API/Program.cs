using MiniApi.Data;
using MiniApi.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddAppLogging();

        builder.Services.AddAppServices();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var databaseInitializer =
                scope.ServiceProvider
                    .GetRequiredService<DatabaseInitializer>();

            databaseInitializer.Initialize();
        }

        app.UseAppMiddleware();

        app.MapAppEndpoints();

        app.Run("http://127.0.0.1:5001");
    }
}
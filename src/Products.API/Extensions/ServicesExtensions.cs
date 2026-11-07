using MiniApi.Clients;
using MiniApi.Data;
using MiniApi.ExceptionHandlers;
using MiniApi.HealthChecks;

namespace MiniApi.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(
        this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();

        services.AddScoped<ItemRepository>();

        services.AddHttpContextAccessor();

        services.AddHttpClient<OrdersClient>();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        services.AddExceptionHandler<
            NotFoundExceptionHandler>();

        services.AddExceptionHandler<
            ValidationExceptionHandler>();

        services.AddExceptionHandler<
            BusinessRuleExceptionHandler>();

        services.AddExceptionHandler<
            GlobalExceptionHandler>();

        services.AddProblemDetails();

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>(
                "sqlite-db",
                tags: new string[] { "database" })
            .AddCheck<ApiStatusCheck>(
                "api-status",
                tags: new string[] { "api" });
    }
}
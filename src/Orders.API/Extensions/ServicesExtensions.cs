using MiniApi.Data;
using MiniApi.HealthChecks;
using Microsoft.AspNetCore.Routing;
using MiniApi.ExceptionHandlers;

namespace MiniApi.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<OrderRepository>();

            // Clientes HTTP hacia los otros microservicios
            services.AddHttpClient("Users", c => c.BaseAddress = new Uri("http://localhost:5002"));
            services.AddHttpClient("Products", c => c.BaseAddress = new Uri("http://localhost:5001"));

           
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<BusinessRuleExceptionHandler>();
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

       
            services.Configure<RouteHandlerOptions>(ConfigurarRouteHandler);
           

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database", "ready"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();
        }

        private static void ConfigurarRouteHandler(RouteHandlerOptions options)
        {
            options.ThrowOnBadRequest = true;
        }
    }
}
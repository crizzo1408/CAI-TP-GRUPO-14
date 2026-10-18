using MiniApi.Data;
using MiniApi.HealthChecks;

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

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600);
                setup.AddHealthCheckEndpoint("MiApi", "/health");
            }).AddInMemoryStorage();
        }
    }
}
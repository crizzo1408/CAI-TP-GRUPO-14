using System.Reflection;
using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Services;
using Cart.API.Swagger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;

namespace Cart.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers + respuesta 400 con el formato de la consigna cuando falla la validación
        services.AddControllers(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = InvalidModelStateHandler.CreateResponse);

        // Capas: repositorio (datos) y service (negocio)
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();

        // Cliente HTTP para Products.API (IHttpClientFactory) que propaga el X-Correlation-Id
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services.AddHttpClient<IProductsClient, ProductsClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:ProductsApi:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("Services:ProductsApi:TimeoutSeconds", 5));
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        // Manejo de errores: primero los handlers específicos, último el global
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddExceptionHandler<ExternalServiceExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Health checks: la base y Products.API forman parte de "ready".
        // Si Products no responde, el estado es Degraded (no Unhealthy).
        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite", tags: ["ready"])
            .AddCheck<ProductsApiHealthCheck>("products-api", failureStatus: HealthStatus.Degraded, tags: ["ready"]);

        // Swagger con comentarios XML y ejemplos de error
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cart.API",
                Version = "v1",
                Description = "Carrito de compras. Valida existencia y stock de productos contra Products.API."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            options.OperationFilter<ErrorExamplesOperationFilter>();
        });
    }
}

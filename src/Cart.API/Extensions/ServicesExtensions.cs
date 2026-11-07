using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.ExceptionHandlers;
using Cart.API.HealthChecks;
using Cart.API.Services;
using Cart.API.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cart.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        // Controllers + respuesta 400 con el formato de la consigna cuando falla la validación
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(ConfigurarRespuestaDeValidacion);

        // Capas: repositorio (datos) y service (negocio)
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();

        // Cliente HTTP para Products.API (IHttpClientFactory) que propaga el X-Correlation-Id.
        // La dirección y el timeout los configura el constructor de ProductsClient.
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services.AddHttpClient<IProductsClient, ProductsClient>()
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
            .AddCheck<SqliteHealthCheck>("sqlite", tags: new string[] { "ready" })
            .AddCheck<ProductsApiHealthCheck>("products-api", failureStatus: HealthStatus.Degraded, tags: new string[] { "ready" });

        // Swagger con comentarios XML y ejemplos de error
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(ConfigurarSwagger);
    }

    /// <summary>
    /// Cuando falla la validación de un DTO, se usa InvalidModelStateHandler
    /// para responder con el formato de la consigna (CRT-004).
    /// </summary>
    private static void ConfigurarRespuestaDeValidacion(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateHandler.CreateResponse;
    }

    private static void ConfigurarSwagger(SwaggerGenOptions options)
    {
        var info = new OpenApiInfo();
        info.Title = "Cart.API";
        info.Version = "v1";
        info.Description = "Carrito de compras. Valida existencia y stock de productos contra Products.API.";
        options.SwaggerDoc("v1", info);

        // Archivo XML que genera el compilador con los comentarios ///
        string xmlPath = Path.Combine(AppContext.BaseDirectory, "Cart.API.xml");
        options.IncludeXmlComments(xmlPath);

        options.OperationFilter<ErrorExamplesOperationFilter>();
    }
}

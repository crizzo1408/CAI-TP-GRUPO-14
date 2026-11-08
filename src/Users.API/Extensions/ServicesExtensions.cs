using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.HealthChecks;
using Users.API.Models;
using Users.API.Services;
using Users.API.Swagger;

namespace Users.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        // Controllers + respuesta 400 con el formato de la consigna cuando falla la validación
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(ConfigurarRespuestaDeValidacion);

        // Capas: repositorio (datos) y service (negocio)
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Manejo de errores: primero los handlers específicos, último el global
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<UnauthorizedExceptionHandler>();
        services.AddExceptionHandler<ForbiddenExceptionHandler>();
        services.AddExceptionHandler<ConflictExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Health checks: la base forma parte de "ready"
        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite", tags: new string[] { "ready" });

        // Swagger con comentarios XML y ejemplos de error
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(ConfigurarSwagger);
    }

    /// <summary>
    /// Cuando falla la validación de un DTO, se usa InvalidModelStateHandler
    /// para responder con el formato de la consigna (USR-002).
    /// </summary>
    private static void ConfigurarRespuestaDeValidacion(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateHandler.CreateResponse;
    }

    private static void ConfigurarSwagger(SwaggerGenOptions options)
    {
        var info = new OpenApiInfo();
        info.Title = "Users.API";
        info.Version = "v1";
        info.Description = "Registro y login de usuarios con bloqueo por intentos fallidos.";
        options.SwaggerDoc("v1", info);

        // Archivo XML que genera el compilador con los comentarios ///
        string xmlPath = Path.Combine(AppContext.BaseDirectory, "Users.API.xml");
        options.IncludeXmlComments(xmlPath);

        options.OperationFilter<ErrorExamplesOperationFilter>();
    }
}

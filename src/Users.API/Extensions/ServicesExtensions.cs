using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.HealthChecks;
using Users.API.Models;
using Users.API.Services;
using Users.API.Swagger;

namespace Users.API.Extensions;

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
            .AddCheck<SqliteHealthCheck>("sqlite", tags: ["ready"]);

        // Swagger con comentarios XML y ejemplos de error
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Users.API",
                Version = "v1",
                Description = "Registro y login de usuarios con bloqueo por intentos fallidos."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            options.OperationFilter<ErrorExamplesOperationFilter>();
        });
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Notifications.API.Clients;
using Notifications.API.Data;
using Notifications.API.ExceptionHandlers;
using Notifications.API.HealthChecks;
using Notifications.API.Services;
using Notifications.API.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notifications.API.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = InvalidModelStateHandler.CreateResponse;
        });

        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddHttpContextAccessor();
        services.AddHttpClient<IUsersClient, UsersClient>();

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite", tags: new string[] { "ready" });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(ConfigureSwagger);
    }

    private static void ConfigureSwagger(SwaggerGenOptions options)
    {
        var info = new OpenApiInfo();
        info.Title = "Notifications.API";
        info.Version = "v1";
        info.Description = "Registro, simulación de envío y consulta de notificaciones.";
        options.SwaggerDoc("v1", info);

        string xmlPath = Path.Combine(AppContext.BaseDirectory, "Notifications.API.xml");
        options.IncludeXmlComments(xmlPath);
        options.OperationFilter<ErrorExamplesOperationFilter>();
    }
}

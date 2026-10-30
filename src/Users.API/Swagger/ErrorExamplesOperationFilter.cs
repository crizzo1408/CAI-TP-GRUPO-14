using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.API.DTOs;
using Users.API.ExceptionHandlers;

namespace Users.API.Swagger;

/// <summary>
/// Lee los [SwaggerErrorExample] de cada endpoint y agrega los ejemplos de error
/// (con errorCode y errorMessage) en la documentación de Swagger.
/// </summary>
public class ErrorExamplesOperationFilter : IOperationFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses is null) return;

        var ejemplos = context.MethodInfo.GetCustomAttributes<SwaggerErrorExampleAttribute>();

        foreach (var grupo in ejemplos.GroupBy(e => e.StatusCode))
        {
            if (!operation.Responses.TryGetValue(grupo.Key.ToString(), out var response)) continue;
            if (response is not OpenApiResponse openApiResponse || openApiResponse.Content is null) continue;
            if (!openApiResponse.Content.TryGetValue("application/json", out var mediaType)) continue;
            if (mediaType is not OpenApiMediaType media) continue;

            media.Examples ??= new Dictionary<string, IOpenApiExample>();

            foreach (var ejemplo in grupo)
            {
                var problem = ProblemDefaults.For(ejemplo.StatusCode);
                var error = new ErrorResponse
                {
                    Type = problem.Type,
                    Title = problem.Title,
                    Status = ejemplo.StatusCode,
                    Detail = problem.Detail,
                    Instance = "/" + context.ApiDescription.RelativePath,
                    ErrorCode = ejemplo.ErrorCode,
                    ErrorMessage = ejemplo.ErrorMessage,
                    CorrelationId = "0f8fad5b-d9cb-469f-a165-70867728950e"
                };

                media.Examples[ejemplo.ErrorCode] = new OpenApiExample
                {
                    Summary = $"{ejemplo.ErrorCode}: {ejemplo.ErrorMessage}",
                    Value = JsonSerializer.SerializeToNode(error, JsonOptions)
                };
            }
        }
    }
}

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
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null)
        {
            return;
        }

        object[] atributos = context.MethodInfo.GetCustomAttributes(typeof(SwaggerErrorExampleAttribute), false);

        foreach (object atributo in atributos)
        {
            var ejemplo = (SwaggerErrorExampleAttribute)atributo;

            // Buscar en la documentación la respuesta de ese status (ej: "404")
            if (!operation.Responses.TryGetValue(ejemplo.StatusCode.ToString(), out var respuesta))
            {
                continue;
            }

            var openApiResponse = respuesta as OpenApiResponse;
            if (openApiResponse == null || openApiResponse.Content == null)
            {
                continue;
            }

            if (!openApiResponse.Content.TryGetValue("application/json", out var contenido))
            {
                continue;
            }

            var media = contenido as OpenApiMediaType;
            if (media == null)
            {
                continue;
            }

            if (media.Examples == null)
            {
                media.Examples = new Dictionary<string, IOpenApiExample>();
            }

            // Armar el ErrorResponse de ejemplo, igual al que devuelve la API
            ProblemInfo problem = ProblemDefaults.For(ejemplo.StatusCode);
            var error = new ErrorResponse();
            error.Type = problem.Type;
            error.Title = problem.Title;
            error.Status = ejemplo.StatusCode;
            error.Detail = problem.Detail;
            error.Instance = "/" + context.ApiDescription.RelativePath;
            error.ErrorCode = ejemplo.ErrorCode;
            error.ErrorMessage = ejemplo.ErrorMessage;
            error.CorrelationId = "0f8fad5b-d9cb-469f-a165-70867728950e";

            var openApiExample = new OpenApiExample();
            openApiExample.Summary = ejemplo.ErrorCode + ": " + ejemplo.ErrorMessage;
            openApiExample.Value = JsonSerializer.SerializeToNode(error, JsonOptions);

            media.Examples[ejemplo.ErrorCode] = openApiExample;
        }
    }
}

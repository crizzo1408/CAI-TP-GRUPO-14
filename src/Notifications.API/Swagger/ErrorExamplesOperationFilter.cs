using System.Text.Json;
using Microsoft.OpenApi;
using Notifications.API.DTOs;
using Notifications.API.ExceptionHandlers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notifications.API.Swagger;

public class ErrorExamplesOperationFilter : IOperationFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null)
        {
            return;
        }

        object[] attributes = context.MethodInfo.GetCustomAttributes(typeof(SwaggerErrorExampleAttribute), false);
        foreach (object attribute in attributes)
        {
            var example = (SwaggerErrorExampleAttribute)attribute;
            if (!operation.Responses.TryGetValue(example.StatusCode.ToString(), out var response))
            {
                continue;
            }

            var openApiResponse = response as OpenApiResponse;
            if (openApiResponse?.Content == null ||
                !openApiResponse.Content.TryGetValue("application/json", out var content))
            {
                continue;
            }

            var media = content as OpenApiMediaType;
            if (media == null)
            {
                continue;
            }

            media.Examples ??= new Dictionary<string, IOpenApiExample>();

            ProblemInfo problem = ProblemDefaults.For(example.StatusCode);
            var error = new ErrorResponse();
            error.Type = problem.Type;
            error.Title = problem.Title;
            error.Status = example.StatusCode;
            error.Detail = problem.Detail;
            error.Instance = "/" + context.ApiDescription.RelativePath;
            error.ErrorCode = example.ErrorCode;
            error.ErrorMessage = example.ErrorMessage;
            error.CorrelationId = "0f8fad5b-d9cb-469f-a165-70867728950e";

            var openApiExample = new OpenApiExample();
            openApiExample.Summary = example.ErrorCode + ": " + example.ErrorMessage;
            openApiExample.Value = JsonSerializer.SerializeToNode(error, JsonOptions);
            media.Examples[example.ErrorCode] = openApiExample;
        }
    }
}

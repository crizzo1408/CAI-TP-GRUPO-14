using System;
using System.Collections.Generic;
using System.Text;
using MiniApi.Data;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints
{
    // Códigos de error de Orders
    public static class OrderErrorCodes
    {
        public const string NotFound = "ORD-001";
        public const string InvalidData = "ORD-002";
        public const string UserNotFound = "ORD-003";
        public const string ProductNotFound = "ORD-004";
        public const string InsufficientStock = "ORD-005";
        public const string InvalidTransition = "ORD-006";
        public const string Internal = "ORD-007";
    }

    public static class OrdersEndpointsExtension
    {
        public static void MapOrderEndpoints(this WebApplication app)
        {
            // GET all (?usuarioId=)
            app.MapGet("/api/orders", (Guid? usuarioId, OrderRepository repo, HttpContext ctx) =>
                Safe(ctx, async () =>
                {
                    var orders = await repo.GetAllAsync(usuarioId);
                    return Results.Ok(orders);
                }))
            .WithTags("Orders");

            // GET by id
            app.MapGet("/api/orders/{id}", (string id, OrderRepository repo, HttpContext ctx) =>
                Safe(ctx, async () =>
                {
                    if (!Guid.TryParse(id, out var orderId))
                        return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");

                    var order = await repo.GetByIdAsync(orderId);
                    return order is not null
                        ? Results.Ok(order)
                        : Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }))
            .WithTags("Orders");

            // POST
            app.MapPost("/api/orders", (CreateOrderRequest req, OrderRepository repo,
                                        IHttpClientFactory http, HttpContext ctx) =>
                Safe(ctx, async () =>
                {
                    // 1. Validar el body (ORD-002)
                    if (req is null || req.UsuarioId == Guid.Empty
                        || req.Items is null || req.Items.Count == 0
                        || req.Items.Any(i => i.ProductoId == Guid.Empty || i.Cantidad <= 0))
                        return Problem(ctx, 400, OrderErrorCodes.InvalidData,
                            "Los datos de la orden son inválidos.");

                    // 2. Validar usuario en Users API (ORD-003)
                    // OJO: la consigna no define GET /api/users/{id}. Ver nota en el chat.
                    var usersResp = await http.CreateClient("Users").GetAsync($"/api/users/{req.UsuarioId}");
                    if (usersResp.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return Problem(ctx, 404, OrderErrorCodes.UserNotFound,
                            "Usuario no encontrado al crear la orden.");
                    usersResp.EnsureSuccessStatusCode();

                    // 3. Validar productos (ORD-004) y stock (ORD-005) en Products API
                    var products = http.CreateClient("Products");
                    var items = new List<OrderItem>();

                    foreach (var item in req.Items)
                    {
                        var prodResp = await products.GetAsync($"/api/products/{item.ProductoId}");
                        if (prodResp.StatusCode == System.Net.HttpStatusCode.NotFound)
                            return Problem(ctx, 404, OrderErrorCodes.ProductNotFound,
                                $"Producto no encontrado: {item.ProductoId}.");
                        prodResp.EnsureSuccessStatusCode();

                        var product = await prodResp.Content.ReadFromJsonAsync<ProductInfo>()
                            ?? throw new InvalidOperationException("Respuesta vacía de Products API.");

                        if (product.Stock < item.Cantidad)
                            return Problem(ctx, 422, OrderErrorCodes.InsufficientStock,
                                $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.");

                        items.Add(new OrderItem
                        {
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = product.Precio   // se captura al crear la orden
                        });
                    }

                    // 4. Guardar (Total y estado Pendiente se calculan en el repositorio)
                    var order = await repo.CreateAsync(req.UsuarioId, items);
                    return Results.Created($"/api/orders/{order.Id}", order);
                }))
            .WithTags("Orders");

            // PUT status
            app.MapPut("/api/orders/{id}/status", (string id, UpdateOrderStatusRequest req,
                                                    OrderRepository repo, HttpContext ctx) =>
                Safe(ctx, async () =>
                {
                    if (req is null || !OrderStatus.TryNormalize(req.Estado, out var nuevoEstado))
                        return Problem(ctx, 400, OrderErrorCodes.InvalidData,
                            $"El estado es inválido. Valores permitidos: {string.Join(", ", OrderStatus.All)}.");

                    if (!Guid.TryParse(id, out var orderId))
                        return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");

                    var order = await repo.GetByIdAsync(orderId);
                    if (order is null)
                        return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");

                    if (!OrderStatus.CanTransition(order.Estado, nuevoEstado))
                        return Problem(ctx, 409, OrderErrorCodes.InvalidTransition,
                            $"Una orden en estado '{order.Estado}' no puede pasar a '{nuevoEstado}'.");

                    var fecha = await repo.UpdateStatusAsync(orderId, nuevoEstado);
                    return fecha is null
                        ? Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.")
                        : Results.Ok(new UpdateOrderStatusResponse(orderId, nuevoEstado, fecha.Value));
                }))
            .WithTags("Orders");
        }

        // Captura cualquier error inesperado y lo devuelve como ORD-007 (500).
        // Cuando se implemente el IExceptionHandler global este wrapper se puede sacar.
        private static async Task<IResult> Safe(HttpContext ctx, Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                ctx.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Orders")
                    .LogError(ex, "Error inesperado en {Path}", ctx.Request.Path);

                return Problem(ctx, 500, OrderErrorCodes.Internal,
                    "Error interno al procesar la orden.");
            }
        }

        // Respuesta de error con el formato de la consigna (sección 3.1)
        private static IResult Problem(HttpContext ctx, int status, string errorCode, string errorMessage)
        {
            var (type, title, detail) = status switch
            {
                400 => ("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud no es válida."),
                404 => ("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
                409 => ("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "No se puede modificar el estado."),
                422 => ("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
                _ => ("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error interno.")
            };

            return Results.Problem(
                type: type,
                title: title,
                detail: detail,
                statusCode: status,
                instance: ctx.Request.Path,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = errorCode,
                    ["errorMessage"] = errorMessage
                });
        }
    }
}
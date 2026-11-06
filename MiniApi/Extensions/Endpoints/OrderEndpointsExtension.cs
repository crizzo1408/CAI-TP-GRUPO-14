using MiniApi.Data;
using MiniApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MiniApi.Extensions.Endpoints
{
    // Catálogo de errores de Orders
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
        
        private static readonly bool UsarServiciosSimulados = true;

      
        private const string UsersApiUrl = "http://localhost:5002";
        private const string ProductsApiUrl = "http://localhost:5001";
        private const string MockApiUrl = "http://localhost:5003/mock";

        private const string CorrelationHeader = "X-Correlation-Id";

        public static void MapOrderEndpoints(this WebApplication app)
        {
            app.MapGet("/api/orders", GetAllOrders)
                .WithTags("Orders")
                .WithSummary("Listar órdenes")
                .WithDescription("Filtro opcional ?usuarioId=. Si el usuarioId no es un GUID válido se devuelve una lista vacía. Errores: ORD-007 (500).")
                .Produces<List<Order>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapGet("/api/orders/{id}", GetOrderById)
                .WithTags("Orders")
                .WithSummary("Obtener detalle de una orden")
                .WithDescription("Errores: ORD-001 (404) orden no encontrada; ORD-007 (500).")
                .Produces<Order>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/orders", CreateOrder)
                .WithTags("Orders")
                .WithSummary("Crear nueva orden")
                .WithDescription("Valida usuario en Users API y productos/stock en Products API. Errores: ORD-002 (400) datos inválidos; ORD-003 (404) usuario no encontrado; ORD-004 (404) producto no encontrado; ORD-005 (422) stock insuficiente; ORD-007 (500).")
                .Produces<Order>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPut("/api/orders/{id}/status", UpdateOrderStatus)
                .WithTags("Orders")
                .WithSummary("Actualizar estado de la orden")
                .WithDescription("Transiciones válidas: Pendiente→Confirmada/Cancelada; Confirmada→Enviada/Cancelada; Enviada→Entregada. Errores: ORD-002 (400) estado inválido; ORD-001 (404) orden no encontrada; ORD-006 (409) transición inválida; ORD-007 (500).")
                .Produces<UpdateOrderStatusResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            // SOLO PARA PROBAR: borrar junto con OrdersMockEndpoints.cs antes de entregar
            if (UsarServiciosSimulados)
            {
                app.MapOrdersMockEndpoints();
            }
        }

        // ───────── GET /api/orders ─────────
        private static async Task<IResult> GetAllOrders(string? usuarioId, OrderRepository repo, HttpContext ctx)
        {
            try
            {
                Guid? filtro = null;
                if (!string.IsNullOrWhiteSpace(usuarioId))
                {
                    Guid idParseado;
                    if (!Guid.TryParse(usuarioId, out idParseado))
                    {
                      
                        return Results.Ok(new List<Order>());
                    }
                    filtro = idParseado;
                }

                List<Order> orders = await repo.GetAllAsync(filtro);
                return Results.Ok(orders);
            }
            catch (Exception ex)
            {
                return InternalError(ctx, ex);
            }
        }

        // ───────── GET /api/orders/{id} ─────────
        private static async Task<IResult> GetOrderById(string id, OrderRepository repo, HttpContext ctx)
        {
            try
            {
                Guid orderId;
                if (!Guid.TryParse(id, out orderId))
                {
                    return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }

                Order? order = await repo.GetByIdAsync(orderId);
                if (order == null)
                {
                    return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }

                return Results.Ok(order);
            }
            catch (Exception ex)
            {
                return InternalError(ctx, ex);
            }
        }

        // ───────── POST /api/orders ─────────
        private static async Task<IResult> CreateOrder(CreateOrderRequest? req, OrderRepository repo,
                                                       IHttpClientFactory httpFactory, HttpContext ctx)
        {
            try
            {
                
                List<string> errores = ValidarCreateRequest(req);
                if (errores.Count > 0)
                {
                    return Problem(ctx, 400, OrderErrorCodes.InvalidData,
                        "Los datos de la orden son inválidos: " + string.Join("; ", errores) + ".");
                }

                CreateOrderRequest pedido = req!; 

                
                List<CreateOrderItemRequest> itemsPedidos = AgruparItems(pedido.Items!);

                HttpClient client = httpFactory.CreateClient();
                string correlationId = GetCorrelationId(ctx);

               
                string userUrl = GetUsersUrl() + "/api/users/" + pedido.UsuarioId;
                using HttpResponseMessage userResp = await GetConCorrelationId(client, userUrl, correlationId);

                if (userResp.StatusCode == HttpStatusCode.NotFound)
                {
                    return Problem(ctx, 404, OrderErrorCodes.UserNotFound,
                        "Usuario no encontrado al crear la orden.");
                }
                userResp.EnsureSuccessStatusCode(); 

                
                List<OrderItem> items = new List<OrderItem>();

                foreach (CreateOrderItemRequest item in itemsPedidos)
                {
                    string productUrl = GetProductsUrl() + "/api/products/" + item.ProductoId;
                    using HttpResponseMessage prodResp = await GetConCorrelationId(client, productUrl, correlationId);

                    if (prodResp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return Problem(ctx, 404, OrderErrorCodes.ProductNotFound,
                            "Producto no encontrado al crear la orden: " + item.ProductoId + ".");
                    }
                    prodResp.EnsureSuccessStatusCode();

                    ProductInfo? product = await prodResp.Content.ReadFromJsonAsync<ProductInfo>();
                    if (product == null)
                    {
                        throw new InvalidOperationException("Products API devolvió una respuesta vacía.");
                    }

                    
                    if (product.Precio <= 0 || product.Id != item.ProductoId)
                    {
                        throw new InvalidOperationException("La respuesta de Products API no respeta el                    contrato (id/precio).");
                    }

                    if (product.Stock < item.Cantidad)
                    {
                        return Problem(ctx, 422, OrderErrorCodes.InsufficientStock,
                            $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.");
                    }

                    items.Add(new OrderItem
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = product.Precio 
                    });
                }

                
                Order order = await repo.CreateAsync(pedido.UsuarioId, items);
                return Results.Created("/api/orders/" + order.Id, order);
            }
            catch (Exception ex)
            {
                return InternalError(ctx, ex);
            }
        }

        // ───────── PUT /api/orders/{id}/status ─────────
        private static async Task<IResult> UpdateOrderStatus(string id, UpdateOrderStatusRequest? req,
                                                             OrderRepository repo, HttpContext ctx)
        {
            try
            {
                string nuevoEstado;
                if (req == null || !OrderStatus.TryNormalize(req.Estado, out nuevoEstado))
                {
                    return Problem(ctx, 400, OrderErrorCodes.InvalidData,
                        "El estado es inválido. Valores permitidos: " + string.Join(", ", OrderStatus.All) + ".");
                }

                Guid orderId;
                if (!Guid.TryParse(id, out orderId))
                {
                    return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }

                Order? order = await repo.GetByIdAsync(orderId);
                if (order == null)
                {
                    return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }

                if (!OrderStatus.CanTransition(order.Estado, nuevoEstado))
                {
                    return Problem(ctx, 409, OrderErrorCodes.InvalidTransition,
                        $"Una orden en estado '{order.Estado}' no puede pasar a '{nuevoEstado}'.");
                }

                DateTime? fecha = await repo.UpdateStatusAsync(orderId, nuevoEstado);
                if (fecha == null)
                {
                    return Problem(ctx, 404, OrderErrorCodes.NotFound, "Orden no encontrada.");
                }

                return Results.Ok(new UpdateOrderStatusResponse(orderId, nuevoEstado, fecha.Value));
            }
            catch (Exception ex)
            {
                return InternalError(ctx, ex);
            }
        }



        private static List<string> ValidarCreateRequest(CreateOrderRequest? req)
        {
            List<string> errores = new List<string>();

            if (req == null)
            {
                errores.Add("el body es obligatorio");
                return errores;
            }

            if (req.UsuarioId == Guid.Empty)
            {
                errores.Add("usuarioId es obligatorio");
            }

            if (req.Items == null || req.Items.Count == 0)
            {
                errores.Add("items no puede estar vacío");
                return errores;
            }

            for (int i = 0; i < req.Items.Count; i++)
            {
                CreateOrderItemRequest item = req.Items[i];
                if (item == null)
                {
                    errores.Add("items[" + i + "] está vacío");
                    continue;
                }
                if (item.ProductoId == Guid.Empty)
                {
                    errores.Add("items[" + i + "].productoId es obligatorio");
                }
                if (item.Cantidad <= 0)
                {
                    errores.Add("items[" + i + "].cantidad debe ser mayor a 0");
                }
            }

            return errores;
        }

        private static List<CreateOrderItemRequest> AgruparItems(List<CreateOrderItemRequest> items)
        {
            List<CreateOrderItemRequest> resultado = new List<CreateOrderItemRequest>();

            foreach (CreateOrderItemRequest item in items)
            {
                bool encontrado = false;
                for (int i = 0; i < resultado.Count; i++)
                {
                    if (resultado[i].ProductoId == item.ProductoId)
                    {
                        resultado[i] = new CreateOrderItemRequest
                        {
                            ProductoId = item.ProductoId,
                            Cantidad = resultado[i].Cantidad + item.Cantidad
                        };
                        encontrado = true;
                        break;
                    }
                }

                if (!encontrado)
                {
                    resultado.Add(new CreateOrderItemRequest
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad
                    });
                }
            }

            return resultado;
        }

        private static string GetUsersUrl()
        {
            if (UsarServiciosSimulados)
            {
                return MockApiUrl;
            }
            return UsersApiUrl;
        }

        private static string GetProductsUrl()
        {
            if (UsarServiciosSimulados)
            {
                return MockApiUrl;
            }
            return ProductsApiUrl;
        }
        private static string GetCorrelationId(HttpContext ctx)
        {
            string valor = ctx.Request.Headers[CorrelationHeader].ToString();
            if (string.IsNullOrEmpty(valor))
            {
                valor = ctx.Response.Headers[CorrelationHeader].ToString();
            }
            if (string.IsNullOrEmpty(valor))
            {
                valor = ctx.TraceIdentifier;
            }
            return valor;
        }

        private static async Task<HttpResponseMessage> GetConCorrelationId(HttpClient client, string url, string correlationId)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(CorrelationHeader, correlationId);
            return await client.SendAsync(request);
        }

        private static ILogger GetLogger(HttpContext ctx)
        {
            return ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Orders");
        }

       
        private static IResult InternalError(HttpContext ctx, Exception ex)
        {
            GetLogger(ctx).LogError(ex, "Error inesperado {ErrorCode} en {Endpoint}",
                OrderErrorCodes.Internal, ctx.Request.Path.Value);

            return Problem(ctx, 500, OrderErrorCodes.Internal, "Error interno al procesar la orden.");
        }

        private static IResult Problem(HttpContext ctx, int status, string errorCode, string errorMessage)
        {
            string type;
            string title;
            string detail;

            if (status == 400)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                title = "Bad Request";
                detail = "La solicitud no es válida.";
            }
            else if (status == 404)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                title = "Not Found";
                detail = "El recurso solicitado no fue encontrado.";
            }
            else if (status == 409)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
                title = "Conflict";
                detail = "No se puede modificar el estado.";
            }
            else if (status == 422)
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                title = "Unprocessable Entity";
                detail = "No se puede procesar la solicitud.";
            }
            else
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                title = "Internal Server Error";
                detail = "Ocurrió un error interno.";
            }

 
            if (status < 500)
            {
                GetLogger(ctx).LogWarning("Error de negocio {ErrorCode} en {Endpoint}: {ErrorMessage}",
                    errorCode, ctx.Request.Path.Value, errorMessage);
            }

            Dictionary<string, object?> extras = new Dictionary<string, object?>();
            extras["errorCode"] = errorCode;
            extras["errorMessage"] = errorMessage;
            extras["correlationId"] = GetCorrelationId(ctx);

            return Results.Problem(
                type: type,
                title: title,
                detail: detail,
                statusCode: status,
                instance: ctx.Request.Path.Value,
                extensions: extras);
        }
    }
}
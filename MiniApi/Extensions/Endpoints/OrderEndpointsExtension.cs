using System.Net;
using System.Net.Http.Json;
using MiniApi.Data;
using MiniApi.ExceptionHandlers;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints
{
    public static class OrdersEndpointsExtension
    {
        private static readonly bool SimularUsers = false;

        private const string UsersApiUrl = "http://localhost:5002";
        private const string ProductsApiUrl = "http://localhost:5001";

        private const string CorrelationHeader = "X-Correlation-Id";

        public static void MapOrderEndpoints(this WebApplication app)
        {
            app.MapGet("/api/orders", GetAllOrders)
                .WithTags("Orders")
                .WithSummary("Listar órdenes")
                .WithDescription("Filtro opcional ?usuarioId=. Si el usuarioId no es un GUID válido se devuelve una lista vacía. Errores: ORD-007 (500).")
                .Produces<List<Order>>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);

            app.MapGet("/api/orders/{id}", GetOrderById)
                .WithTags("Orders")
                .WithSummary("Obtener detalle de una orden")
                .WithDescription("Errores: ORD-001 (404) orden no encontrada; ORD-007 (500).")
                .Produces<Order>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/orders", CreateOrder)
                .WithTags("Orders")
                .WithSummary("Crear nueva orden")
                .WithDescription("Valida usuario en Users API y productos/stock en Products API. Errores: ORD-002 (400) datos inválidos; ORD-003 (404) usuario no encontrado; ORD-004 (404) producto no encontrado; ORD-005 (422) stock insuficiente; ORD-007 (500).")
                .Produces<Order>(StatusCodes.Status201Created)
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);

            app.MapPut("/api/orders/{id}/status", UpdateOrderStatus)
                .WithTags("Orders")
                .WithSummary("Actualizar estado de la orden")
                .WithDescription("Transiciones válidas: Pendiente→Confirmada/Cancelada; Confirmada→Enviada/Cancelada; Enviada→Entregada. Errores: ORD-002 (400) estado inválido; ORD-001 (404) orden no encontrada; ORD-006 (409) transición inválida; ORD-007 (500).")
                .Produces<UpdateOrderStatusResponse>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);

            
           
        }

        // ───────── GET /api/orders ─────────
        private static async Task<IResult> GetAllOrders(string? usuarioId, OrderRepository repo)
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

        // ───────── GET /api/orders/{id} ─────────
        private static async Task<IResult> GetOrderById(string id, OrderRepository repo)
        {
            Guid orderId;
            if (!Guid.TryParse(id, out orderId))
            {
                throw new NotFoundException(OrderErrorCodes.NotFound, "Orden no encontrada.");
            }

            Order? order = await repo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException(OrderErrorCodes.NotFound, "Orden no encontrada.");
            }

            return Results.Ok(order);
        }

        // ───────── POST /api/orders ─────────
        private static async Task<IResult> CreateOrder(CreateOrderRequest? req, OrderRepository repo,
                                                       IHttpClientFactory httpFactory, HttpContext ctx)
        {
            // 1. Validar el body (ORD-002)
            List<string> errores = ValidarCreateRequest(req);
            if (errores.Count > 0)
            {
                throw new ValidationException(OrderErrorCodes.InvalidData,
                    "Los datos de la orden son inválidos: " + string.Join("; ", errores) + ".");
            }

            CreateOrderRequest pedido = req!;

            
            List<CreateOrderItemRequest> itemsPedidos = AgruparItems(pedido.Items!);

            HttpClient client = httpFactory.CreateClient();
            string correlationId = ErrorResponseFactory.GetCorrelationId(ctx);

            // 2. Validar usuario en Users API (ORD-003)
            
            string userUrl = GetUsersUrl(ctx) + "/api/users/" + pedido.UsuarioId;
            using HttpResponseMessage userResp = await GetConCorrelationId(client, userUrl, correlationId);

            if (userResp.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException(OrderErrorCodes.UserNotFound,
                    "Usuario no encontrado al crear la orden.");
            }
            userResp.EnsureSuccessStatusCode(); // otro error de Users API -> GlobalExceptionHandler -> ORD-007

            // 3. Validar productos (ORD-004) y stock (ORD-005) en Products API
            List<OrderItem> items = new List<OrderItem>();

            foreach (CreateOrderItemRequest item in itemsPedidos)
            {
                string productUrl = ProductsApiUrl + "/api/products/" + item.ProductoId;
                using HttpResponseMessage prodResp = await GetConCorrelationId(client, productUrl, correlationId);

                if (prodResp.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(OrderErrorCodes.ProductNotFound,
                        "Producto no encontrado al crear la orden: " + item.ProductoId + ".");
                }
                prodResp.EnsureSuccessStatusCode();

                ProductInfo? product = await prodResp.Content.ReadFromJsonAsync<ProductInfo>();
                if (product == null)
                {
                    throw new InvalidOperationException("Products API devolvió una respuesta vacía.");
                }

                // Apéndice A
                
                if (product.Precio <= 0 || product.Id != item.ProductoId)
                {
                    throw new InvalidOperationException("La respuesta de Products API no respeta el contrato (id/precio).");
                }

                if (product.Stock < item.Cantidad)
                {
                    throw new BusinessRuleException(OrderErrorCodes.InsufficientStock,
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.",
                        StatusCodes.Status422UnprocessableEntity);
                }

                items.Add(new OrderItem
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = product.Precio // se captura al crear la orden
                });
            }

            // 4. Guardar (Total y estado Pendiente se calculan en el repositorio)
            Order order = await repo.CreateAsync(pedido.UsuarioId, items);
            return Results.Created("/api/orders/" + order.Id, order);
        }

        // ───────── PUT /api/orders/{id}/status ─────────
        private static async Task<IResult> UpdateOrderStatus(string id, UpdateOrderStatusRequest? req, OrderRepository repo)
        {
            string nuevoEstado;
            if (req == null || !OrderStatus.TryNormalize(req.Estado, out nuevoEstado))
            {
                throw new ValidationException(OrderErrorCodes.InvalidData,
                    "El estado es inválido. Valores permitidos: " + string.Join(", ", OrderStatus.All) + ".");
            }

            Guid orderId;
            if (!Guid.TryParse(id, out orderId))
            {
                throw new NotFoundException(OrderErrorCodes.NotFound, "Orden no encontrada.");
            }

            Order? order = await repo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException(OrderErrorCodes.NotFound, "Orden no encontrada.");
            }

            if (!OrderStatus.CanTransition(order.Estado, nuevoEstado))
            {
                throw new BusinessRuleException(OrderErrorCodes.InvalidTransition,
                    $"Una orden en estado '{order.Estado}' no puede pasar a '{nuevoEstado}'.",
                    StatusCodes.Status409Conflict);
            }

            DateTime? fecha = await repo.UpdateStatusAsync(orderId, nuevoEstado);
            if (fecha == null)
            {
                throw new NotFoundException(OrderErrorCodes.NotFound, "Orden no encontrada.");
            }

            return Results.Ok(new UpdateOrderStatusResponse(orderId, nuevoEstado, fecha.Value));
        }

        // ───────── Auxiliares ─────────

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

        // Arma la URL del mock con el mismo host y puerto donde está corriendo la app
        private static string GetMockUrl(HttpContext ctx)
        {
            return ctx.Request.Scheme + "://" + ctx.Request.Host + "/mock";
        }

        private static string GetUsersUrl(HttpContext ctx)
        {
            if (SimularUsers)
            {
                return GetMockUrl(ctx);
            }
            return UsersApiUrl;
        }

        // GET a otro microservicio propagando el X-Correlation-Id (sección 5.5)
        private static async Task<HttpResponseMessage> GetConCorrelationId(HttpClient client, string url, string correlationId)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(CorrelationHeader, correlationId);
            return await client.SendAsync(request);
        }
    }
}
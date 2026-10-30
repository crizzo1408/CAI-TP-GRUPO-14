# Cart.API

Carrito de compras de cada usuario. Para agregar o modificar productos consulta a Products.API si el producto existe y si hay stock.

| Dato | Valor |
|---|---|
| Puerto | **5004** (HTTP) |
| Swagger | http://localhost:5004/swagger |
| Base de datos | SQLite `cart.db` (se crea sola al iniciar) |
| Depende de | **Products.API** en `http://localhost:5001/` (configurable en `appsettings.json` → `Services:ProductsApi:BaseUrl`) |
| Logs | Consola + `logs/cart-AAAAMMDD.json` |

## Endpoints

| Método | Endpoint | Descripción | Status posibles |
|---|---|---|---|
| GET | `/api/cart/{userId}` | Obtener carrito del usuario | 200, 404, 500 |
| POST | `/api/cart/{userId}/items` | Agregar producto al carrito | 200, 400, 404, 422, 500 |
| PUT | `/api/cart/{userId}/items/{productId}` | Actualizar cantidad de un item | 200, 400, 404, 422, 500 |
| DELETE | `/api/cart/{userId}/items/{productId}` | Quitar un producto del carrito | 204, 404, 500 |
| DELETE | `/api/cart/{userId}` | Vaciar carrito completo | 204, 404, 500 |
| GET | `/health` | Estado general | 200, 503 |
| GET | `/health/ready` | Listo para atender (SQLite + Products.API) | 200, 503 |
| GET | `/health/live` | Proceso vivo | 200 |

## Códigos de error

| errorCode | HTTP | errorMessage | Cuándo |
|---|---|---|---|
| CRT-001 | 404 | Carrito no encontrado. | GET/PUT/DELETE cuando el usuario no tiene carrito, o PUT/DELETE de un producto que no está en el carrito. |
| CRT-002 | 404 | Producto no encontrado. | POST/PUT cuando el producto no existe en Products.API. |
| CRT-003 | 422 | Stock insuficiente para agregar al carrito. | POST/PUT cuando la cantidad supera el stock. Ej: "Stock insuficiente. Disponible: 1, solicitado: 5." |
| CRT-004 | 400 | Cantidad inválida. | POST/PUT con cantidad ≤ 0. También cualquier otro dato inválido del request (falta el productoId, JSON inválido, userId con formato inválido en el POST). |
| CRT-005 | 500 | Error interno al procesar el carrito. | Error inesperado, o Products.API caído / con error 5xx. |

## Reglas de negocio

- El primer POST crea el carrito del usuario.
- Si se agrega un producto que ya estaba, se **suma** la cantidad y el stock se valida contra el total.
- El PUT **reemplaza** la cantidad.
- Vaciar el carrito lo deja sin productos, pero el carrito sigue existiendo (un GET devuelve 200 con `items: []`).
- `fechaActualizacion` se actualiza en cada operación.
- Un `userId` o `productId` con formato inválido en la ruta se trata como "no encontrado" (CRT-001), salvo en el POST, que responde CRT-004.

## Comunicación con Products.API

- Usa `IHttpClientFactory` con un cliente propio (`Clients/ProductsClient.cs`) y DTO propio. No referencia el proyecto Products.
- Llama a `GET /api/products/{id}`: 200 → usa `stock`; 404 → CRT-002; cualquier otro status, timeout (5 s) o conexión rechazada → CRT-005.
- Reenvía el header `X-Correlation-Id` del request original, así el mismo id aparece en los logs de los dos servicios.
- `/health/ready` informa **Degraded** si Products.API no responde.

using Cart.API.Constants;
using Cart.API.DTOs;
using Cart.API.Services;
using Cart.API.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

/// <summary>
/// Carrito de compras de cada usuario.
/// El controller no tiene lógica: recibe el request, llama al service y devuelve el resultado.
/// Los errores los lanza el service como excepciones y los resuelven los IExceptionHandler.
/// </summary>
[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public class CartController(ICartService cartService) : ControllerBase
{
    /// <summary>
    /// Obtiene el carrito de un usuario.
    /// </summary>
    /// <param name="userId">Id del usuario (Guid).</param>
    /// <response code="200">Carrito del usuario.</response>
    /// <response code="404">CRT-001: el usuario no tiene carrito.</response>
    /// <response code="500">CRT-005: error interno.</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001, ErrorMessages.CRT_001)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005, ErrorMessages.CRT_005)]
    public async Task<ActionResult<CartResponse>> Get(string userId)
    {
        return Ok(await cartService.GetAsync(userId));
    }

    /// <summary>
    /// Agrega un producto al carrito.
    /// </summary>
    /// <remarks>
    /// Si el usuario no tiene carrito, se crea. Si el producto ya estaba, se suma la cantidad.
    /// Se valida contra Products.API que el producto exista y que haya stock para la cantidad total.
    /// </remarks>
    /// <param name="userId">Id del usuario (Guid).</param>
    /// <param name="request">Producto y cantidad.</param>
    /// <response code="200">Carrito actualizado.</response>
    /// <response code="400">CRT-004: cantidad menor o igual a cero, o datos faltantes.</response>
    /// <response code="404">CRT-002: el producto no existe en Products.API.</response>
    /// <response code="422">CRT-003: stock insuficiente.</response>
    /// <response code="500">CRT-005: error interno o Products.API no disponible.</response>
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.CRT_004, "Cantidad inválida. Debe ser mayor a cero (recibido: 0).")]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_002, "Producto no encontrado: '3fa85f64-5717-4562-b3fc-2c963f66afa6'.")]
    [SwaggerErrorExample(StatusCodes.Status422UnprocessableEntity, ErrorCodes.CRT_003, "Stock insuficiente. Disponible: 1, solicitado: 5.")]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005, "No se pudo comunicar con Products.API para validar el producto.")]
    public async Task<ActionResult<CartResponse>> AddItem(string userId, [FromBody] AddCartItemRequest request)
    {
        return Ok(await cartService.AddItemAsync(userId, request));
    }

    /// <summary>
    /// Actualiza la cantidad de un producto del carrito.
    /// </summary>
    /// <remarks>
    /// La cantidad nueva reemplaza a la anterior. Se valida el stock contra Products.API.
    /// </remarks>
    /// <param name="userId">Id del usuario (Guid).</param>
    /// <param name="productId">Id del producto (Guid).</param>
    /// <param name="request">Nueva cantidad.</param>
    /// <response code="200">Carrito actualizado.</response>
    /// <response code="400">CRT-004: cantidad menor o igual a cero.</response>
    /// <response code="404">CRT-001: no hay carrito o el producto no está en él. CRT-002: el producto ya no existe en Products.API.</response>
    /// <response code="422">CRT-003: stock insuficiente.</response>
    /// <response code="500">CRT-005: error interno o Products.API no disponible.</response>
    [HttpPut("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.CRT_004, "Cantidad inválida. Debe ser mayor a cero (recibido: -1).")]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001, ErrorMessages.CRT_001)]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_002, "Producto no encontrado: '3fa85f64-5717-4562-b3fc-2c963f66afa6'.")]
    [SwaggerErrorExample(StatusCodes.Status422UnprocessableEntity, ErrorCodes.CRT_003, "Stock insuficiente. Disponible: 1, solicitado: 5.")]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005, ErrorMessages.CRT_005)]
    public async Task<ActionResult<CartResponse>> UpdateItem(string userId, string productId, [FromBody] UpdateCartItemRequest request)
    {
        return Ok(await cartService.UpdateItemAsync(userId, productId, request));
    }

    /// <summary>
    /// Quita un producto del carrito.
    /// </summary>
    /// <param name="userId">Id del usuario (Guid).</param>
    /// <param name="productId">Id del producto (Guid).</param>
    /// <response code="204">Producto quitado.</response>
    /// <response code="404">CRT-001: no hay carrito o el producto no está en él.</response>
    /// <response code="500">CRT-005: error interno.</response>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001, ErrorMessages.CRT_001)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005, ErrorMessages.CRT_005)]
    public async Task<IActionResult> RemoveItem(string userId, string productId)
    {
        await cartService.RemoveItemAsync(userId, productId);
        return NoContent();
    }

    /// <summary>
    /// Vacía el carrito completo.
    /// </summary>
    /// <remarks>
    /// El carrito sigue existiendo, sin productos: un GET posterior devuelve 200 con la lista vacía.
    /// </remarks>
    /// <param name="userId">Id del usuario (Guid).</param>
    /// <response code="204">Carrito vaciado.</response>
    /// <response code="404">CRT-001: el usuario no tiene carrito.</response>
    /// <response code="500">CRT-005: error interno.</response>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001, ErrorMessages.CRT_001)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005, ErrorMessages.CRT_005)]
    public async Task<IActionResult> Clear(string userId)
    {
        await cartService.ClearAsync(userId);
        return NoContent();
    }
}

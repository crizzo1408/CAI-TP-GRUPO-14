using Microsoft.AspNetCore.Mvc;
using Notifications.API.Constants;
using Notifications.API.DTOs;
using Notifications.API.Services;
using Notifications.API.Swagger;

namespace Notifications.API.Controllers;

/// <summary>Envío y consulta de notificaciones de usuarios.</summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Registra una notificación y simula su envío.</summary>
    /// <param name="request">Usuario, mensaje y canal de envío.</param>
    /// <param name="cancellationToken">Permite cancelar la solicitud.</param>
    /// <response code="201">Notificación registrada y enviada.</response>
    /// <response code="400">NTF-002: faltan datos o el tipo no es Email, Push o SMS.</response>
    /// <response code="404">NTF-001: el usuario no existe en Users.API.</response>
    /// <response code="500">NTF-004: error interno o Users.API no está disponible.</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.NTF_002, "El tipo debe ser Email, Push o SMS.")]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.NTF_001, ErrorMessages.NTF_001)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004, ErrorMessages.NTF_004)]
    public async Task<ActionResult<NotificationResponse>> Send(
        [FromBody] SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        NotificationResponse response = await _notificationService.SendAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Lista todas las notificaciones de un usuario.</summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <response code="200">Lista ordenada de la más reciente a la más antigua.</response>
    /// <response code="404">NTF-003: el usuario no tiene notificaciones.</response>
    /// <response code="500">NTF-004: error interno.</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(List<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status404NotFound, ErrorCodes.NTF_003, ErrorMessages.NTF_003)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004, ErrorMessages.NTF_004)]
    public async Task<ActionResult<List<NotificationResponse>>> GetByUserId(Guid userId)
    {
        List<NotificationResponse> response = await _notificationService.GetByUserIdAsync(userId);
        return Ok(response);
    }
}

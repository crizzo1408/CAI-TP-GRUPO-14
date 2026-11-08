using Microsoft.AspNetCore.Mvc;
using Users.API.Constants;
using Users.API.DTOs;
using Users.API.Services;
using Users.API.Swagger;

namespace Users.API.Controllers;

/// <summary>
/// Registro e inicio de sesión de usuarios.
/// El controller no tiene lógica: recibe el request, llama al service y devuelve el resultado.
/// Los errores los lanza el service como excepciones y los resuelven los IExceptionHandler.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <remarks>
    /// El email debe ser único y la contraseña tener al menos 8 caracteres.
    /// La contraseña se guarda hasheada y nunca se devuelve.
    /// </remarks>
    /// <param name="request">Datos del usuario.</param>
    /// <response code="201">Usuario registrado.</response>
    /// <response code="400">USR-002: datos faltantes o con formato incorrecto.</response>
    /// <response code="409">USR-001: el email ya está registrado.</response>
    /// <response code="500">USR-006: error interno.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.USR_002, "El email no tiene un formato válido; La contraseña debe tener al menos 8 caracteres.")]
    [SwaggerErrorExample(StatusCodes.Status409Conflict, ErrorCodes.USR_001, "El email 'maria@email.com' ya está registrado.")]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.USR_006, ErrorMessages.USR_006)]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        UserResponse user = await _userService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    /// <summary>
    /// Autentica un usuario con email y contraseña.
    /// </summary>
    /// <remarks>
    /// Al acumular 3 intentos fallidos consecutivos la cuenta se bloquea (USR-004).
    /// Un login correcto resetea el contador de intentos.
    /// Usuario de prueba bloqueado manualmente (solo en Development): bloqueado@email.com / Password123! → USR-005.
    /// </remarks>
    /// <param name="request">Credenciales.</param>
    /// <response code="200">Login correcto.</response>
    /// <response code="400">USR-002: falta el email o la contraseña.</response>
    /// <response code="401">USR-003: email o contraseña incorrectos.</response>
    /// <response code="403">USR-004: bloqueado por intentos fallidos. USR-005: bloqueado por fraude.</response>
    /// <response code="500">USR-006: error interno.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.USR_002, "La contraseña es obligatoria.")]
    [SwaggerErrorExample(StatusCodes.Status401Unauthorized, ErrorCodes.USR_003, ErrorMessages.USR_003)]
    [SwaggerErrorExample(StatusCodes.Status403Forbidden, ErrorCodes.USR_004, ErrorMessages.USR_004)]
    [SwaggerErrorExample(StatusCodes.Status403Forbidden, ErrorCodes.USR_005, ErrorMessages.USR_005)]
    [SwaggerErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.USR_006, ErrorMessages.USR_006)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        LoginResponse user = await _userService.LoginAsync(request);
        return Ok(user);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Users.API.Constants;
using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

/// <summary>
/// Lógica de negocio de usuarios: registro, login y bloqueo por intentos fallidos.
/// Cuando algo no se cumple lanza una excepción de dominio con el código del catálogo;
/// el IExceptionHandler correspondiente arma la respuesta HTTP.
/// </summary>
public class UserService : IUserService
{
    /// <summary>Cantidad de intentos fallidos consecutivos que bloquean al usuario.</summary>
    public const int MaxIntentosFallidos = 3;

    private readonly IUserRepository _repository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, IPasswordHasher<User> passwordHasher, ILogger<UserService> logger)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
    {
        // Los campos ya vienen validados por Data Annotations (formato, obligatorios, largo)
        string email = NormalizeEmail(request.Email);

        bool existe = await _repository.ExistsByEmailAsync(email);
        if (existe)
        {
            throw new ConflictException(ErrorCodes.USR_001, "El email '" + email + "' ya está registrado.");
        }

        var user = new User();
        user.Id = Guid.NewGuid();
        user.Nombre = request.Nombre.Trim();
        user.Apellido = request.Apellido.Trim();
        user.Email = email;
        user.FechaRegistro = DateTime.UtcNow;
        user.Activo = true;
        user.IntentosFallidos = 0;

        // La contraseña nunca se guarda en texto plano: solo su hash
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        try
        {
            await _repository.CreateAsync(user);
        }
        catch (SqliteException ex)
        {
            // 19 = violación de UNIQUE: dos requests registraron el mismo email al mismo tiempo
            if (ex.SqliteErrorCode == 19)
            {
                throw new ConflictException(ErrorCodes.USR_001, "El email '" + email + "' ya está registrado.");
            }
            throw;
        }

        _logger.LogInformation("Usuario registrado {UserId} ({Email})", user.Id, user.Email);

        var response = new UserResponse();
        response.Id = user.Id;
        response.Nombre = user.Nombre;
        response.Apellido = user.Apellido;
        response.Email = user.Email;
        response.FechaRegistro = user.FechaRegistro;
        response.Activo = user.Activo;
        return response;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        string email = NormalizeEmail(request.Email);
        User user = await _repository.GetByEmailAsync(email);

        // Email inexistente: mismo error que contraseña incorrecta,
        // para no revelar qué emails están registrados.
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.USR_003, ErrorMessages.USR_003);
        }

        // Usuario ya bloqueado: no se valida la contraseña
        if (!user.Activo)
        {
            throw CrearExcepcionBloqueo(user);
        }

        PasswordVerificationResult resultado = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            user.IntentosFallidos = user.IntentosFallidos + 1;

            if (user.IntentosFallidos >= MaxIntentosFallidos)
            {
                // Tercer intento fallido: se bloquea la cuenta
                user.Activo = false;
                await _repository.UpdateLoginStateAsync(user.Id, user.Activo, user.IntentosFallidos);
                _logger.LogWarning("Usuario {UserId} bloqueado tras {Intentos} intentos fallidos", user.Id, user.IntentosFallidos);
                throw new ForbiddenException(ErrorCodes.USR_004, ErrorMessages.USR_004);
            }

            await _repository.UpdateLoginStateAsync(user.Id, user.Activo, user.IntentosFallidos);
            _logger.LogInformation("Login fallido para {UserId}: intento {Intentos} de {Max}",
                user.Id, user.IntentosFallidos, MaxIntentosFallidos);
            throw new UnauthorizedException(ErrorCodes.USR_003, ErrorMessages.USR_003);
        }

        // Login correcto: se resetean los intentos fallidos
        if (user.IntentosFallidos > 0)
        {
            await _repository.UpdateLoginStateAsync(user.Id, user.Activo, 0);
        }

        _logger.LogInformation("Login exitoso {UserId}", user.Id);

        var response = new LoginResponse();
        response.Id = user.Id;
        response.Nombre = user.Nombre;
        response.Apellido = user.Apellido;
        response.Email = user.Email;
        return response;
    }

    /// <summary>
    /// Busca un usuario por id. Lo usan Orders (ORD-003) y Notifications (NTF-001)
    /// para validar que el usuario exista. Si está bloqueado se devuelve igual,
    /// con Activo = false: cada servicio decide qué hacer.
    /// </summary>
    public async Task<UserResponse> GetByIdAsync(string id)
    {
        // Un id que no es un Guid válido no puede existir: se responde 404 igual
        Guid userId;
        if (!Guid.TryParse(id, out userId))
        {
            throw new NotFoundException(ErrorCodes.USR_007, ErrorMessages.USR_007);
        }

        User user = await _repository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException(ErrorCodes.USR_007, ErrorMessages.USR_007);
        }

        var response = new UserResponse();
        response.Id = user.Id;
        response.Nombre = user.Nombre;
        response.Apellido = user.Apellido;
        response.Email = user.Email;
        response.FechaRegistro = user.FechaRegistro;
        response.Activo = user.Activo;
        return response;
    }

    /// <summary>
    /// Un usuario inactivo puede estar bloqueado por dos motivos:
    /// - acumuló 3 o más intentos fallidos → USR-004
    /// - lo bloquearon manualmente (fraude) sin intentos fallidos → USR-005
    /// </summary>
    private static ForbiddenException CrearExcepcionBloqueo(User user)
    {
        if (user.IntentosFallidos >= MaxIntentosFallidos)
        {
            return new ForbiddenException(ErrorCodes.USR_004, ErrorMessages.USR_004);
        }

        return new ForbiddenException(ErrorCodes.USR_005, ErrorMessages.USR_005);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}

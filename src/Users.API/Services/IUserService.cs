using Users.API.DTOs;

namespace Users.API.Services;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetByIdAsync(string id);
    Task<bool> ExistsByIdAsync(Guid id);
}

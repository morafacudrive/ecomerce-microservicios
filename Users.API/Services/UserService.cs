using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

public class UserService(UserRepository repo)
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ValidationException("USR-002", "El nombre es requerido.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            throw new ValidationException("USR-002", "El apellido es requerido.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("USR-002", "El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("USR-002", "La contraseña es requerida.");

        var existe = await repo.GetByEmailAsync(request.Email);

        if (existe != null)
            throw new ConflictException("USR-001", $"El email '{request.Email}' ya está registrado.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = request.Password,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        await repo.CreateAsync(user);

        return MapToResponse(user);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request)
    {
        if (request == null)
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("USR-002", "El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("USR-002", "La contraseña es requerida.");

        var user = await repo.GetByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");

        if (!user.Activo && user.IntentosFallidos >= 3)
            throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos.");

        if (!user.Activo)
            throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad.");

        if (user.PasswordHash != request.Password)
        {
            user.IntentosFallidos++;
            if (user.IntentosFallidos >= 3)
                user.Activo = false;

            await repo.UpdateAsync(user);

            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        user.IntentosFallidos = 0;
        await repo.UpdateAsync(user);

        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }
}

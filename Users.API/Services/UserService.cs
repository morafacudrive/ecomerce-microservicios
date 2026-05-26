using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

public class UserService
{
    private static List<User> _users = new();

    public UserResponse Register(RegisterRequest request)
    {
        // Validar email duplicado
        var existe = _users.Any(u => u.Email == request.Email);
        if (existe)
            throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

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

        _users.Add(user);
        return MapToResponse(user);
    }

    public UserResponse Login(LoginRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Email == request.Email);

        // Usuario no existe o contraseña incorrecta
        if (user == null || user.PasswordHash != request.Password)
        {
            if (user != null)
            {
                user.IntentosFallidos++;
                if (user.IntentosFallidos >= 3)
                    user.Activo = false;
            }
            throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
        }

        // Usuario bloqueado por intentos fallidos
        if (!user.Activo && user.IntentosFallidos >= 3)
            throw new BusinessRuleException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

        // Usuario bloqueado por fraude
        if (!user.Activo)
            throw new BusinessRuleException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

        // Login exitoso - resetear intentos fallidos
        user.IntentosFallidos = 0;
        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Apellido = u.Apellido,
        Email = u.Email,
        FechaRegistro = u.FechaRegistro,
        Activo = u.Activo
    };
}
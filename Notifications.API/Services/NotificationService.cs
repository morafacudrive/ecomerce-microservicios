using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService(NotificationRepository repo)
{
    private static readonly List<string> TiposValidos = new()
    {
        "Email",
        "Push",
        "SMS"
    };

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
    {
        if (request.UsuarioId == Guid.Empty)
            throw new ValidationException("NTF-002", "El UsuarioId es requerido.");

        if (string.IsNullOrWhiteSpace(request.Mensaje))
            throw new ValidationException("NTF-002", "El mensaje es requerido.");

        if (request.Mensaje.Length > 500)
            throw new ValidationException("NTF-002", "El mensaje no puede superar los 500 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Tipo))
            throw new ValidationException("NTF-002", "El tipo de notificación es requerido.");

        if (!TiposValidos.Contains(request.Tipo))
            throw new ValidationException("NTF-002", $"El tipo '{request.Tipo}' no es válido. Use: Email, Push o SMS.");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        await repo.CreateAsync(notification);

        return MapToResponse(notification);
    }

    public async Task<List<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("NTF-002", "El UsuarioId es requerido.");

        var notifications = await repo.GetByUsuarioIdAsync(userId.ToString());

        if (!notifications.Any())
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

        return notifications.Select(MapToResponse).ToList();
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
}
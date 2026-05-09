using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService
{
    private static List<Notification> _notifications = new();

    private static readonly List<string> TiposValidos = new() { "Email", "Push", "SMS" };

    public NotificationResponse Send(SendNotificationRequest request)
    {
        if (string.IsNullOrEmpty(request.Mensaje))
            throw new BusinessRuleException("NTF-002", "El mensaje es requerido.");

        if (!TiposValidos.Contains(request.Tipo))
            throw new BusinessRuleException("NTF-002", $"El tipo '{request.Tipo}' no es válido. Use: Email, Push o SMS.");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        _notifications.Add(notification);
        return MapToResponse(notification);
    }

    public List<NotificationResponse> GetByUserId(Guid userId)
    {
        var notifications = _notifications.Where(n => n.UsuarioId == userId).ToList();
        if (!notifications.Any())
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");

        return notifications.Select(MapToResponse).ToList();
    }

    private static NotificationResponse MapToResponse(Notification n) => new()
    {
        Id = n.Id,
        UsuarioId = n.UsuarioId,
        Mensaje = n.Mensaje,
        Tipo = n.Tipo,
        Estado = n.Estado,
        FechaEnvio = n.FechaEnvio
    };
}
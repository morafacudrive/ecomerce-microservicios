using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

public class SendNotificationRequest
{
    [Required(ErrorMessage = "El UsuarioId es requerido.")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "El mensaje es requerido.")]
    [MaxLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de notificación es requerido.")]
    public string Tipo { get; set; } = string.Empty;
}
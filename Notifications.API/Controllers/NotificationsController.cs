using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _service;

    public NotificationsController(NotificationService service)
    {
        _service = service;
    }

    [HttpPost("send")]
    public IActionResult Send([FromBody] SendNotificationRequest request)
    {
        var notification = _service.Send(request);
        return CreatedAtAction(nameof(GetByUserId), new { userId = notification.UsuarioId }, notification);
    }

    [HttpGet("{userId}")]
    public IActionResult GetByUserId(Guid userId)
    {
        var notifications = _service.GetByUserId(userId);
        return Ok(notifications);
    }
}
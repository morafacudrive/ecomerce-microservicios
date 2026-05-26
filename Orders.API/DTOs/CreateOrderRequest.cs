using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "El UsuarioId es requerido.")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "La orden debe tener al menos un item.")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
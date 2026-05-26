using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "El ProductoId es requerido.")]
    public Guid ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
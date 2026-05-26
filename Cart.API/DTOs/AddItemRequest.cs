using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public class AddItemRequest
{
    [Required(ErrorMessage = "El ProductoId es requerido.")]
    public Guid ProductoId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Cantidad inválida.")]
    public int Cantidad { get; set; }
}
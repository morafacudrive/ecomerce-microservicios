using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

public class UpdateItemRequest
{
    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Cantidad inválida.")]
    public int Cantidad { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "El estado es requerido.")]
    public string Estado { get; set; } = string.Empty;
}
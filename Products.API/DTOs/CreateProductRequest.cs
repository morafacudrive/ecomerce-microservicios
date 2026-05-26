using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs;

public class CreateProductRequest
{
    [Required(ErrorMessage = "El nombre del producto es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre del producto no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es requerido.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "La categoría es requerida.")]
    public string Categoria { get; set; } = string.Empty;
}
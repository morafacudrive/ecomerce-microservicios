namespace Cart.API.Models;

public class CartItem
{
    public Guid UsuarioId { get; set; }
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
}
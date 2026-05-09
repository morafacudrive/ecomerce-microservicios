using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

public class CartService
{
    private static List<Cart.API.Models.Cart> _carts = new();

    public CartResponse GetByUserId(Guid userId)
    {
        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        return MapToResponse(cart);
    }

    public CartResponse AddItem(Guid userId, AddItemRequest request)
    {
        if (request.Cantidad <= 0)
            throw new BusinessRuleException("CRT-004", "La cantidad debe ser mayor a cero.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
        if (cart == null)
        {
            cart = new Cart.API.Models.Cart
            {
                UsuarioId = userId,
                Items = new List<CartItem>(),
                FechaActualizacion = DateTime.UtcNow
            };
            _carts.Add(cart);
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
        if (item == null)
            cart.Items.Add(new CartItem { ProductoId = request.ProductoId, Cantidad = request.Cantidad });
        else
            item.Cantidad += request.Cantidad;

        cart.FechaActualizacion = DateTime.UtcNow;
        return MapToResponse(cart);
    }

    public CartResponse UpdateItem(Guid userId, Guid productId, UpdateItemRequest request)
    {
        if (request.Cantidad <= 0)
            throw new BusinessRuleException("CRT-004", "La cantidad debe ser mayor a cero.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);
        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

        item.Cantidad = request.Cantidad;
        cart.FechaActualizacion = DateTime.UtcNow;
        return MapToResponse(cart);
    }

    public void RemoveItem(Guid userId, Guid productId)
    {
        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);
        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito.");

        cart.Items.Remove(item);
        cart.FechaActualizacion = DateTime.UtcNow;
    }

    public void ClearCart(Guid userId)
    {
        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        cart.Items.Clear();
        cart.FechaActualizacion = DateTime.UtcNow;
    }

    private static CartResponse MapToResponse(Cart.API.Models.Cart c) => new()
    {
        UsuarioId = c.UsuarioId,
        Items = c.Items.Select(i => new CartItemResponse
        {
            ProductoId = i.ProductoId,
            Cantidad = i.Cantidad
        }).ToList(),
        FechaActualizacion = c.FechaActualizacion
    };
}
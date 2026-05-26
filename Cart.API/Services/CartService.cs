using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

public class CartService
{
    private static readonly List<Cart.API.Models.Cart> _carts = new();

    public CartResponse GetByUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        return MapToResponse(cart);
    }

    public CartResponse AddItem(Guid userId, AddItemRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        if (request == null)
            throw new ValidationException("CRT-004", "Los datos del item son inválidos.");

        if (request.ProductoId == Guid.Empty)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        if (request.Cantidad <= 0)
            throw new ValidationException("CRT-004", "Cantidad inválida.");

        // CRT-003 queda pendiente para cuando validen stock contra Products.API.
        // Ejemplo futuro:
        // if (request.Cantidad > stockDisponible)
        //     throw new UnprocessableException("CRT-003", $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {request.Cantidad}.");

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
        {
            cart.Items.Add(new CartItem
            {
                ProductoId = request.ProductoId,
                Cantidad = request.Cantidad
            });
        }
        else
        {
            item.Cantidad += request.Cantidad;
        }

        cart.FechaActualizacion = DateTime.UtcNow;

        return MapToResponse(cart);
    }

    public CartResponse UpdateItem(Guid userId, Guid productId, UpdateItemRequest request)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        if (productId == Guid.Empty)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        if (request == null)
            throw new ValidationException("CRT-004", "Los datos del item son inválidos.");

        if (request.Cantidad <= 0)
            throw new ValidationException("CRT-004", "Cantidad inválida.");

        // CRT-003 queda pendiente para cuando validen stock contra Products.API.
        // Ejemplo futuro:
        // if (request.Cantidad > stockDisponible)
        //     throw new UnprocessableException("CRT-003", $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {request.Cantidad}.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        item.Cantidad = request.Cantidad;
        cart.FechaActualizacion = DateTime.UtcNow;

        return MapToResponse(cart);
    }

    public void RemoveItem(Guid userId, Guid productId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        if (productId == Guid.Empty)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        cart.Items.Remove(item);
        cart.FechaActualizacion = DateTime.UtcNow;
    }

    public void ClearCart(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        cart.Items.Clear();
        cart.FechaActualizacion = DateTime.UtcNow;
    }

    private static CartResponse MapToResponse(Cart.API.Models.Cart cart)
    {
        return new CartResponse
        {
            UsuarioId = cart.UsuarioId,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad
            }).ToList(),
            FechaActualizacion = cart.FechaActualizacion
        };
    }
}
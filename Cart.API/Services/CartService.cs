using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

public class CartService(CartRepository repo)
{
    public async Task<CartResponse> GetByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        var cart = await repo.GetByUsuarioIdAsync(userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        return MapToResponse(cart);
    }
    
    public async Task<CartResponse> AddItemAsync(Guid userId, AddItemRequest request)
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
        // if (request.Cantidad > stockDisponible)
        //     throw new UnprocessableException("CRT-003", $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {request.Cantidad}.");

        var cart = await repo.GetByUsuarioIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart.API.Models.Cart
            {
                UsuarioId = userId,
                Items = new List<CartItem>(),
                FechaActualizacion = DateTime.UtcNow
            };
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);

        if (item == null)
        {
            cart.Items.Add(new CartItem
            {
                UsuarioId = userId,
                ProductoId = request.ProductoId,
                Cantidad = request.Cantidad
            });
        }
        else
        {
            item.Cantidad += request.Cantidad;
        }

        cart.FechaActualizacion = DateTime.UtcNow;

        await repo.SaveAsync(cart);

        return MapToResponse(cart);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateItemRequest request)
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
        // if (request.Cantidad > stockDisponible)
        //     throw new UnprocessableException("CRT-003", $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {request.Cantidad}.");

        var cart = await repo.GetByUsuarioIdAsync(userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        item.Cantidad = request.Cantidad;
        cart.FechaActualizacion = DateTime.UtcNow;

        await repo.SaveAsync(cart);

        return MapToResponse(cart);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        if (productId == Guid.Empty)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        var cart = await repo.GetByUsuarioIdAsync(userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

        if (item == null)
            throw new NotFoundException("CRT-002", "Producto no encontrado.");

        cart.Items.Remove(item);
        cart.FechaActualizacion = DateTime.UtcNow;

        await repo.SaveAsync(cart);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("CRT-004", "El UsuarioId es requerido.");

        var cart = await repo.GetByUsuarioIdAsync(userId);

        if (cart == null)
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");

        await repo.DeleteAsync(userId);
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
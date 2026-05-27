using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public class OrderService(OrderRepository repo)
{
    private static readonly List<string> EstadosValidos = new()
    {
        "Pendiente",
        "Confirmada",
        "Enviada",
        "Entregada",
        "Cancelada"
    };

    private static readonly List<string> TransicionesValidas = new()
    {
        "Pendiente->Confirmada",
        "Confirmada->Enviada",
        "Enviada->Entregada",
        "Pendiente->Cancelada",
        "Confirmada->Cancelada"
    };

    public async Task<List<OrderResponse>> GetAllAsync(Guid? usuarioId)
    {
        var orders = await repo.GetAllAsync(usuarioId?.ToString());
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id)
    {
        var order = await repo.GetByIdAsync(id.ToString());

        if (order == null)
            throw new NotFoundException("ORD-001", "Orden no encontrada.");

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
    {
        ValidateCreateOrderRequest(request);

        // ORD-003 queda pendiente para validar contra Users.API.
        // if (!usuarioExiste)
        //     throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");

        // ORD-004 y ORD-005 quedan pendientes para validar contra Products.API.
        // if (!productoExiste)
        //     throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");
        //
        // if (cantidadSolicitada > stockDisponible)
        //     throw new UnprocessableException("ORD-005", "Stock insuficiente para uno o más productos.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Empty, // se asigna abajo
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = 0
            }).ToList(),
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        foreach (var item in order.Items)
            item.OrderId = order.Id;

        order.Total = order.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

        await repo.CreateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        if (request == null)
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

        if (string.IsNullOrWhiteSpace(request.Estado))
            throw new ValidationException("ORD-002", "El estado es requerido.");

        if (!EstadosValidos.Contains(request.Estado))
            throw new ValidationException("ORD-002", $"El estado '{request.Estado}' no es válido.");

        var order = await repo.GetByIdAsync(id.ToString());

        if (order == null)
            throw new NotFoundException("ORD-001", "Orden no encontrada.");

        var transicion = $"{order.Estado}->{request.Estado}";

        if (!TransicionesValidas.Contains(transicion))
            throw new ConflictException("ORD-006", $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");

        await repo.UpdateEstadoAsync(id.ToString(), request.Estado);

        order.Estado = request.Estado;

        return MapToResponse(order);
    }

    private static void ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (request == null)
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

        if (request.UsuarioId == Guid.Empty)
            throw new ValidationException("ORD-002", "El UsuarioId es requerido.");

        if (request.Items == null || !request.Items.Any())
            throw new ValidationException("ORD-002", "La orden debe tener al menos un item.");

        foreach (var item in request.Items)
        {
            if (item.ProductoId == Guid.Empty)
                throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

            if (item.Cantidad <= 0)
                throw new ValidationException("ORD-002", "La cantidad debe ser mayor a 0.");
        }
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Items = order.Items.Select(i => new OrderIte
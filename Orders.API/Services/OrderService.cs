using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public class OrderService
{
    private static List<Order> _orders = new();

    private static readonly List<string> TransicionesValidas = new()
    {
        "Pendiente->Confirmada",
        "Confirmada->Enviada",
        "Enviada->Entregada",
        "Pendiente->Cancelada",
        "Confirmada->Cancelada"
    };

    public List<OrderResponse> GetAll(Guid? usuarioId)
    {
        var query = _orders.AsQueryable();
        if (usuarioId.HasValue)
            query = query.Where(o => o.UsuarioId == usuarioId);
        return query.Select(MapToResponse).ToList();
    }

    public OrderResponse GetById(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new NotFoundException("ORD-001", "Orden no encontrada.");
        return MapToResponse(order);
    }

    public OrderResponse Create(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new BusinessRuleException("ORD-002", "La orden debe tener al menos un item.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = 0 // Se actualizará cuando se integre con Products.API
            }).ToList(),
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        order.Total = order.Items.Sum(i => i.Cantidad * i.PrecioUnitario);
        _orders.Add(order);
        return MapToResponse(order);
    }

    public OrderResponse UpdateStatus(Guid id, UpdateOrderStatusRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new NotFoundException("ORD-001", "Orden no encontrada.");

        var transicion = $"{order.Estado}->{request.Estado}";
        if (!TransicionesValidas.Contains(transicion))
            throw new BusinessRuleException("ORD-006", $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");

        order.Estado = request.Estado;
        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order o) => new()
    {
        Id = o.Id,
        UsuarioId = o.UsuarioId,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            ProductoId = i.ProductoId,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario
        }).ToList(),
        Total = o.Total,
        Estado = o.Estado,
        FechaCreacion = o.FechaCreacion
    };
}
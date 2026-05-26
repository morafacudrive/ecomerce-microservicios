using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public class OrderService
{
    private static readonly List<Order> _orders = new();

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
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = 0
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
        if (request == null)
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos.");

        if (string.IsNullOrWhiteSpace(request.Estado))
            throw new ValidationException("ORD-002", "El estado es requerido.");

        if (!EstadosValidos.Contains(request.Estado))
            throw new ValidationException("ORD-002", $"El estado '{request.Estado}' no es válido.");

        var order = _orders.FirstOrDefault(o => o.Id == id);

        if (order == null)
            throw new NotFoundException("ORD-001", "Orden no encontrada.");

        var transicion = $"{order.Estado}->{request.Estado}";

        if (!TransicionesValidas.Contains(transicion))
            throw new ConflictException("ORD-006", $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");

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
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion
        };
    }
}
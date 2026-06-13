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

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        ValidateCreateOrderRequest(request);

        var client = httpClientFactory.CreateClient();
        var productsUrl = config["ServicesUrls:ProductsAPI"] ?? "https://localhost:7000";

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = new List<OrderItem>(),
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var response = await client.GetAsync($"{productsUrl}/api/products/{item.ProductoId}");

            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

            var json = await response.Content.ReadAsStringAsync();
            var product = System.Text.Json.JsonSerializer.Deserialize<ProductoDto>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = product!.Precio
            });
        }

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
public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
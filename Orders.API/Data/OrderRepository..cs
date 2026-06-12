using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data;

public class OrderRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db");

    public async Task<IEnumerable<Order>> GetAllAsync(string? usuarioId)
    {
        using var conn = CreateConnection();
        var sql = "SELECT * FROM Orders";
        if (!string.IsNullOrEmpty(usuarioId)) sql += " WHERE UsuarioId = @UsuarioId";
        var orders = (await conn.QueryAsync<Order>(sql, new { UsuarioId = usuarioId })).ToList();
        foreach (var order in orders)
            order.Items = (await GetItemsAsync(conn, order.Id.ToString())).ToList();
        return orders;
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        using var conn = CreateConnection();
        var order = await conn.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id", new { Id = id });
        if (order is not null)
            order.Items = (await GetItemsAsync(conn, id)).ToList();
        return order;
    }

    public async Task<IEnumerable<Order>> GetActivesByProductoIdAsync(string productoId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Order>("""
            SELECT DISTINCT o.* FROM Orders o
            INNER JOIN OrderItems oi ON o.Id = oi.OrderId
            WHERE oi.ProductoId = @ProductoId
            AND o.Estado IN ('Pendiente', 'Confirmada')
        """, new { ProductoId = productoId });
    }

    public async Task<Order> CreateAsync(Order order)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("""
            INSERT INTO Orders (Id, UsuarioId, Total, Estado, FechaCreacion)
            VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
        """, order, tx);

        foreach (var item in order.Items)
            await conn.ExecuteAsync("""
                INSERT INTO OrderItems (Id, OrderId, ProductoId, Cantidad, PrecioUnitario)
                VALUES (@Id, @OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
            """, item, tx);

        tx.Commit();
        return order;
    }

    public async Task UpdateEstadoAsync(string id, string estado)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Orders SET Estado = @Estado WHERE Id = @Id",
            new { Id = id, Estado = estado });
    }

    private async Task<IEnumerable<OrderItem>> GetItemsAsync(SqliteConnection conn, string orderId) =>
        await conn.QueryAsync<OrderItem>(
            "SELECT * FROM OrderItems WHERE OrderId = @OrderId", new { OrderId = orderId });
}
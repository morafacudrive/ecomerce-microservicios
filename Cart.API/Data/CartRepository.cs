using Dapper;
using Microsoft.Data.Sqlite;
using CartModel = Cart.API.Models.Cart;
using CartItem = Cart.API.Models.CartItem;

namespace Cart.API.Data;

public class CartRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db");

    public async Task<CartModel?> GetByUsuarioIdAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();
        var cart = await conn.QueryFirstOrDefaultAsync<CartModel>(
            "SELECT * FROM Carts WHERE UsuarioId = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() });

        if (cart is not null)
        {
            var items = await conn.QueryAsync<CartItem>(
                "SELECT * FROM CartItems WHERE UsuarioId = @UsuarioId",
                new { UsuarioId = usuarioId.ToString() });
            cart.Items = items.ToList();
        }

        return cart;
    }

    public async Task SaveAsync(CartModel cart)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("""
            INSERT INTO Carts (UsuarioId, FechaActualizacion)
            VALUES (@UsuarioId, @FechaActualizacion)
            ON CONFLICT(UsuarioId) DO UPDATE SET FechaActualizacion = @FechaActualizacion
        """, new { UsuarioId = cart.UsuarioId.ToString(), cart.FechaActualizacion }, tx);

        await conn.ExecuteAsync(
            "DELETE FROM CartItems WHERE UsuarioId = @UsuarioId",
            new { UsuarioId = cart.UsuarioId.ToString() }, tx);

        foreach (var item in cart.Items)
            await conn.ExecuteAsync("""
                INSERT INTO CartItems (UsuarioId, ProductoId, Cantidad)
                VALUES (@UsuarioId, @ProductoId, @Cantidad)
            """, new
            {
                UsuarioId = cart.UsuarioId.ToString(),
                ProductoId = item.ProductoId.ToString(),
                item.Cantidad
            }, tx);

        tx.Commit();
    }

    public async Task DeleteAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(
            "DELETE FROM CartItems WHERE UsuarioId = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, tx);
        await conn.ExecuteAsync(
            "DELETE FROM Carts WHERE UsuarioId = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, tx);
        tx.Commit();
    }
}
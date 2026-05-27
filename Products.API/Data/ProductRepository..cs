using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data;

public class ProductRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db");

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();
        var sql = "SELECT * FROM Products WHERE 1=1";
        if (!string.IsNullOrEmpty(categoria)) sql += " AND Categoria = @Categoria";
        if (!string.IsNullOrEmpty(nombre)) sql += " AND Nombre LIKE @Nombre";
        return await conn.QueryAsync<Product>(sql, new { Categoria = categoria, Nombre = $"%{nombre}%" });
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", new { Id = id });
    }

    public async Task<Product?> GetByNombreYCategoriaAsync(string nombre, string categoria)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Nombre = @Nombre AND Categoria = @Categoria",
            new { Nombre = nombre, Categoria = categoria });
    }

    public async Task<Product> CreateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO Products (Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, product);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE Products SET
                Nombre      = @Nombre,
                Descripcion = @Descripcion,
                Precio      = @Precio,
                Stock       = @Stock,
                Categoria   = @Categoria
            WHERE Id = @Id
        """, product);
        return product;
    }

    public async Task DeleteAsync(string id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM Products WHERE Id = @Id", new { Id = id });
    }
}
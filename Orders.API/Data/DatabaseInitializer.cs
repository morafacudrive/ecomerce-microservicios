using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data;

public class DatabaseInitializer(IConfiguration config)
{
    public void Initialize()
    {
        var connStr = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=orders.db";

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Orders (
                Id            TEXT PRIMARY KEY,
                UsuarioId     TEXT NOT NULL,
                Total         REAL NOT NULL,
                Estado        TEXT NOT NULL,
                FechaCreacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS OrderItems (
                Id             TEXT PRIMARY KEY,
                OrderId        TEXT NOT NULL,
                ProductoId     TEXT NOT NULL,
                Cantidad       INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id)
            );
        """);
    }
}
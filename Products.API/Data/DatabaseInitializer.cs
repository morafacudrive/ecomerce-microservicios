using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API.Data;

public class DatabaseInitializer(IConfiguration config)
{
    public void Initialize()
    {
        var connStr = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=products.db";

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Products (
                Id            TEXT PRIMARY KEY,
                Nombre        TEXT NOT NULL,
                Descripcion   TEXT,
                Precio        REAL NOT NULL,
                Stock         INTEGER NOT NULL,
                Categoria     TEXT NOT NULL,
                FechaCreacion TEXT NOT NULL
            );
        """);
    }
}
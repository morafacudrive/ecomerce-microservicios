using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;

namespace Cart.API.Data;

public class DatabaseInitializer(IConfiguration config)
{
    public void Initialize()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        var connStr = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=cart.db";

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Carts (
                UsuarioId          TEXT PRIMARY KEY,
                FechaActualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS CartItems (
                UsuarioId  TEXT NOT NULL,
                ProductoId TEXT NOT NULL,
                Cantidad   INTEGER NOT NULL,
                PRIMARY KEY (UsuarioId, ProductoId),
                FOREIGN KEY (UsuarioId) REFERENCES Carts(UsuarioId)
            );
        """);
    }
}

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        => parameter.Value = value.ToString();

    public override Guid Parse(object value)
        => Guid.Parse((string)value);
}
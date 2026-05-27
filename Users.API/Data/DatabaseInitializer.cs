using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Data;

public class DatabaseInitializer(IConfiguration config)
{
    public void Initialize()
    {
        var connStr = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=users.db";

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Users (
                Id               TEXT PRIMARY KEY,
                Nombre           TEXT NOT NULL,
                Apellido         TEXT NOT NULL,
                Email            TEXT NOT NULL UNIQUE,
                PasswordHash     TEXT NOT NULL,
                FechaRegistro    TEXT NOT NULL,
                Activo           INTEGER NOT NULL DEFAULT 1,
                IntentosFallidos INTEGER NOT NULL DEFAULT 0
            );
        """);
    }
}
using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

public class DatabaseInitializer(IConfiguration config)
{
    public void Initialize()
    {
        var connStr = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=notifications.db";

        using var conn = new SqliteConnection(connStr);
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS Notifications (
                Id         TEXT PRIMARY KEY,
                UsuarioId  TEXT NOT NULL,
                Mensaje    TEXT NOT NULL,
                Tipo       TEXT NOT NULL,
                Estado     TEXT NOT NULL,
                FechaEnvio TEXT NOT NULL
            );
        """);
    }
}
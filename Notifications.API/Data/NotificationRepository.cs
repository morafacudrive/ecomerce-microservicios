using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data;

public class NotificationRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db");

    public async Task<IEnumerable<Notification>> GetByUsuarioIdAsync(string usuarioId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Notification>(
            "SELECT * FROM Notifications WHERE UsuarioId = @UsuarioId ORDER BY FechaEnvio DESC",
            new { UsuarioId = usuarioId });
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO Notifications (Id, UsuarioId, Mensaje, Tipo, Estado, FechaEnvio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
        """, notification);
        return notification;
    }
}
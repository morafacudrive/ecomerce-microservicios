using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

public class UserRepository(IConfiguration config)
{
    private SqliteConnection CreateConnection() =>
        new(config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db");

    public async Task<User?> GetByIdAsync(string id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO Users (Id, Nombre, Apellido, Email, PasswordHash, FechaRegistro, Activo, IntentosFallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
        """, user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE Users SET
                Activo           = @Activo,
                IntentosFallidos = @IntentosFallidos
            WHERE Id = @Id
        """, user);
    }
}
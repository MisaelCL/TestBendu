using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace C_C.Repositories;

public sealed class PerfilRepository : RepositoryBase, IPerfilRepository
{
    private readonly ILogger<PerfilRepository> _logger;

    public PerfilRepository(IConnectionFactory connectionFactory, ILogger<PerfilRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public Task<Perfil?> GetAsync(int ID_Perfil, CancellationToken ct = default)
    {
        return GetInternalAsync("SELECT ID_Perfil, ID_Cuenta, Nickname, Nombre, Apellido_Paterno, Apellido_Materno, Genero, Fecha_Nacimiento, Carrera, Biografia, Foto_Principal, Fecha_Creacion, Fecha_Actualizacion FROM dbo.Perfil WHERE ID_Perfil = @Perfil", new SqlParameter[] { P("@Perfil", ID_Perfil) }, ct);
    }

    public Task<Perfil?> GetByNickAsync(string nik, CancellationToken ct = default)
    {
        return GetInternalAsync("SELECT ID_Perfil, ID_Cuenta, Nickname, Nombre, Apellido_Paterno, Apellido_Materno, Genero, Fecha_Nacimiento, Carrera, Biografia, Foto_Principal, Fecha_Creacion, Fecha_Actualizacion FROM dbo.Perfil WHERE Nickname = @Nick", new SqlParameter[] { P("@Nick", nik) }, ct);
    }

    public Task<int> UpdateAsync(Perfil p, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"UPDATE dbo.Perfil
SET Nickname = @Nickname,
    Nombre = @Nombre,
    Apellido_Paterno = @ApellidoP,
    Apellido_Materno = @ApellidoM,
    Genero = @Genero,
    Fecha_Nacimiento = @FechaNacimiento,
    Carrera = @Carrera,
    Biografia = @Biografia,
    Foto_Principal = @Foto,
    Fecha_Actualizacion = SYSUTCDATETIME()
WHERE ID_Perfil = @Perfil";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(new[]
            {
                P("@Nickname", p.Nickname),
                P("@Nombre", p.Nombre),
                P("@ApellidoP", p.Apellido_Paterno),
                P("@ApellidoM", p.Apellido_Materno ?? (object)DBNull.Value),
                P("@Genero", p.Genero),
                P("@FechaNacimiento", p.Fecha_Nacimiento),
                P("@Carrera", p.Carrera),
                P("@Biografia", p.Biografia ?? (object)DBNull.Value),
                P("@Foto", p.Foto_Principal ?? (object)DBNull.Value),
                P("@Perfil", p.ID_Perfil)
            });

            var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Perfil {PerfilId} actualizado. Filas afectadas: {Rows}", p.ID_Perfil, rows);
            return rows;
        }, ct);
    }

    private Task<Perfil?> GetInternalAsync(string sql, SqlParameter[] parameters, CancellationToken ct)
    {
        return WithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return new Perfil
                {
                    ID_Perfil = reader.GetInt32(0),
                    ID_Cuenta = reader.GetInt32(1),
                    Nickname = reader.GetString(2),
                    Nombre = reader.GetString(3),
                    Apellido_Paterno = reader.GetString(4),
                    Apellido_Materno = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Genero = reader.GetString(6),
                    Fecha_Nacimiento = reader.GetDateTime(7),
                    Carrera = reader.GetString(8),
                    Biografia = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Foto_Principal = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Fecha_Creacion = reader.GetDateTime(11),
                    Fecha_Actualizacion = reader.GetDateTime(12)
                };
            }

            return null;
        }, ct);
    }
}

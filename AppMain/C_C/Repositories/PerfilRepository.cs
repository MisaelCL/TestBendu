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
        return GetInternalAsync("SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion FROM dbo.Perfil WHERE ID_Perfil = @Perfil", new SqlParameter[] { P("@Perfil", ID_Perfil) }, ct);
    }

    public Task<Perfil?> GetByNickAsync(string nik, CancellationToken ct = default)
    {
        return GetInternalAsync("SELECT ID_Perfil, ID_Cuenta, Nikname, Biografia, Foto_Perfil, Fecha_Creacion FROM dbo.Perfil WHERE Nikname = @Nick", new SqlParameter[] { P("@Nick", nik) }, ct);
    }

    public Task<int> UpdateAsync(Perfil p, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"UPDATE dbo.Perfil
SET Nikname = @Nikname,
    Biografia = @Biografia,
    Foto_Perfil = @Foto
WHERE ID_Perfil = @Perfil";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(new[]
            {
                P("@Nikname", p.Nikname),
                P("@Biografia", p.Biografia ?? (object)DBNull.Value),
                P("@Foto", p.Foto_Perfil ?? (object)DBNull.Value),
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
                    Nikname = reader.GetString(2),
                    Biografia = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Foto_Perfil = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Fecha_Creacion = reader.GetDateTime(5)
                };
            }

            return null;
        }, ct);
    }
}

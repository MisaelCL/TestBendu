using System.Globalization;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace C_C.Repositories;

public sealed class MatchRepository : RepositoryBase, IMatchRepository
{
    private readonly ILogger<MatchRepository> _logger;

    public MatchRepository(IConnectionFactory connectionFactory, ILogger<MatchRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public Task<int> CrearAsync(int Perfil_Emisor, int Perfil_Receptor, string Estado, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"INSERT INTO dbo.[Match] (Perfil_Emisor, Perfil_Receptor, Estado)
VALUES (@Emisor, @Receptor, @Estado);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Emisor", Perfil_Emisor));
            command.Parameters.Add(P("@Receptor", Perfil_Receptor));
            command.Parameters.Add(P("@Estado", Estado));
            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var id = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            _logger.LogInformation("Match {MatchId} creado entre {Emisor} y {Receptor}", id, Perfil_Emisor, Perfil_Receptor);
            return id;
        }, ct);
    }

    public Task<int> ActualizarEstadoAsync(int ID_Match, string Estado, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"UPDATE dbo.[Match]
SET Estado = @Estado
WHERE ID_Match = @Match";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Estado", Estado));
            command.Parameters.Add(P("@Match", ID_Match));
            var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Match {MatchId} actualizado a estado {Estado}. Filas: {Rows}", ID_Match, Estado, rows);
            return rows;
        }, ct);
    }

    public Task<bool> ExisteParAsync(int a, int b, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"SELECT TOP 1 1 FROM dbo.[Match] WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B) OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@A", a));
            command.Parameters.Add(P("@B", b));
            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is not null;
        }, ct);
    }
}

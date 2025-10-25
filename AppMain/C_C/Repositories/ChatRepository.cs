using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace C_C.Repositories;

public sealed class ChatRepository : RepositoryBase, IChatRepository
{
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(IConnectionFactory connectionFactory, ILogger<ChatRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public Task<int> CrearChatPorMatchAsync(int ID_Match, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = "INSERT INTO dbo.Chat (ID_Match, Fecha_Creacion) VALUES (@Match, SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Match", ID_Match));
            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var id = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            _logger.LogInformation("Chat creado para match {MatchId} con ID {ChatId}", ID_Match, id);
            return id;
        }, ct);
    }

    public Task<(DateTime? LastAt, long? LastId)> ObtenerUltimoMensajeAsync(int ID_Chat, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = "SELECT LastMessageAtUtc, LastMessageId FROM dbo.Chat WHERE ID_Chat = @Chat";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Chat", ID_Chat));
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var lastAt = reader.IsDBNull(0) ? null : reader.GetDateTime(0);
                var lastId = reader.IsDBNull(1) ? null : reader.GetInt64(1);
                return (lastAt, lastId);
            }

            return (null, null);
        }, ct);
    }

    public Task<IEnumerable<(int ID_Chat, int ID_Match, DateTime Fecha_Creacion, DateTime? LastAt, long? LastId)>> ListarChatsPorPerfilAsync(int ID_Perfil, int top, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"SELECT TOP(@Top) c.ID_Chat, c.ID_Match, c.Fecha_Creacion, c.LastMessageAtUtc, c.LastMessageId
FROM dbo.Chat AS c
JOIN dbo.Match AS m ON m.ID_Match = c.ID_Match
WHERE m.Perfil_Emisor = @Perfil OR m.Perfil_Receptor = @Perfil
ORDER BY CASE WHEN c.LastMessageAtUtc IS NULL THEN 1 ELSE 0 END,
         c.LastMessageAtUtc DESC, c.Fecha_Creacion DESC;";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Top", top));
            command.Parameters.Add(P("@Perfil", ID_Perfil));
            var items = new List<(int, int, DateTime, DateTime?, long?)>();
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                items.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2), reader.IsDBNull(3) ? null : reader.GetDateTime(3), reader.IsDBNull(4) ? null : reader.GetInt64(4)));
            }

            return items;
        }, ct);
    }

    public Task<(int PerfilA, int PerfilB, int ID_Match)?> ObtenerParticipantesAsync(int ID_Chat, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"SELECT m.Perfil_Emisor, m.Perfil_Receptor, m.ID_Match
FROM dbo.Chat AS c
JOIN dbo.Match AS m ON m.ID_Match = c.ID_Match
WHERE c.ID_Chat = @Chat";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Chat", ID_Chat));
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2));
            }

            return null;
        }, ct);
    }

    public Task<int> ActualizarUltimoMensajeAsync(int ID_Chat, long mensajeId, DateTime fechaUtc, CancellationToken ct = default)
    {
        return WithConnectionAsync(connection => ActualizarUltimoMensajeAsyncInternal(ID_Chat, mensajeId, fechaUtc, connection, null, ct), ct);
    }

    public Task<int> ActualizarUltimoMensajeAsync(int ID_Chat, long mensajeId, DateTime fechaUtc, SqlConnection connection, SqlTransaction? transaction, CancellationToken ct = default)
    {
        return ActualizarUltimoMensajeAsyncInternal(ID_Chat, mensajeId, fechaUtc, connection, transaction, ct);
    }

    private async Task<int> ActualizarUltimoMensajeAsyncInternal(int ID_Chat, long mensajeId, DateTime fechaUtc, SqlConnection connection, SqlTransaction? transaction, CancellationToken ct)
    {
        const string sql = @"UPDATE dbo.Chat
SET LastMessageId = @Mensaje,
    LastMessageAtUtc = @Fecha
WHERE ID_Chat = @Chat";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add(P("@Mensaje", mensajeId));
        command.Parameters.Add(P("@Fecha", fechaUtc));
        command.Parameters.Add(P("@Chat", ID_Chat));
        var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        _logger.LogInformation("Chat {ChatId} actualizado con último mensaje {MensajeId}", ID_Chat, mensajeId);
        return rows;
    }
}

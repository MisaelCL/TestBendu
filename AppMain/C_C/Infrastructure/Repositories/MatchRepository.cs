using C_C.Application.Repositories;
using C_C.Domain;
using C_C.Infrastructure.Common;
using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Repositories;

public sealed class MatchRepository : RepositoryBase, IMatchRepository
{
    public MatchRepository(SqlConnectionFactory? connectionFactory = null)
        : base(connectionFactory)
    {
    }

    public Task<Match?> GetByIdAsync(int idMatch, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match
                                                   FROM dbo.Match WHERE ID_Match = @Id", cn);
            cmd.Parameters.AddWithValue("@Id", idMatch);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return null;
            }

            return MapMatch(reader);
        }, ct);

    public Task<bool> ExistsAsync(int idPerfilA, int idPerfilB, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"SELECT 1 FROM dbo.Match
                                                   WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B)
                                                      OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)", cn);
            cmd.Parameters.AddWithValue("@A", idPerfilA);
            cmd.Parameters.AddWithValue("@B", idPerfilB);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is not null;
        }, ct);

    public Task<IReadOnlyList<Match>> ListByPerfilAsync(int idPerfil, int page, int pageSize, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            var offset = Math.Max(page, 0) * Math.Max(pageSize, 1);
            await using var cmd = new SqlCommand(@"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match
                                                   FROM dbo.Match
                                                   WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil
                                                   ORDER BY Fecha_Match DESC, ID_Match DESC
                                                   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", cn);
            cmd.Parameters.AddWithValue("@Perfil", idPerfil);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", Math.Max(pageSize, 1));
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var matches = new List<Match>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                matches.Add(MapMatch(reader));
            }

            return (IReadOnlyList<Match>)matches;
        }, ct);

    public Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default)
        => WithConnectionAsync(cn => CreateMatchAsync(cn, null, idPerfilEmisor, idPerfilReceptor, estado, ct), ct);

    public Task<bool> UpdateEstadoAsync(int idMatch, string nuevoEstado, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand("UPDATE dbo.Match SET Estado = @Estado WHERE ID_Match = @Id", cn);
            cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
            cmd.Parameters.AddWithValue("@Id", idMatch);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return rows > 0;
        }, ct);

    public Task<bool> DeleteMatchAsync(int idMatch, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand("DELETE FROM dbo.Match WHERE ID_Match = @Id", cn);
            cmd.Parameters.AddWithValue("@Id", idMatch);
            var rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return rows > 0;
        }, ct);

    public Task<int> EnsureChatForMatchAsync(int idMatch, CancellationToken ct = default)
        => WithConnectionAsync(cn => EnsureChatForMatchAsync(cn, null, idMatch, ct), ct);

    public Task<Chat?> GetChatByMatchIdAsync(int idMatch, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var cmd = new SqlCommand(@"SELECT ID_Chat, ID_Match, Fecha_Creacion, LastMessageAtUtc, LastMessageId
                                                   FROM dbo.Chat WHERE ID_Match = @MatchId", cn);
            cmd.Parameters.AddWithValue("@MatchId", idMatch);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return null;
            }

            return MapChat(reader);
        }, ct);

    public Task<long> AddMensajeAsync(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            await using var tx = await cn.BeginTransactionAsync(ct).ConfigureAwait(false);
            try
            {
                var id = await AddMensajeAsync(cn, tx, idChat, idRemitentePerfil, contenido, confirmacionLectura, ct).ConfigureAwait(false);
                await tx.CommitAsync(ct).ConfigureAwait(false);
                return id;
            }
            catch
            {
                await tx.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }, ct);

    public Task<IReadOnlyList<Mensaje>> ListMensajesAsync(int idChat, int page, int pageSize, CancellationToken ct = default)
        => WithConnectionAsync(async cn =>
        {
            var offset = Math.Max(page, 0) * Math.Max(pageSize, 1);
            await using var cmd = new SqlCommand(@"SELECT ID_Mensaje, ID_Chat, Remitente, Contenido, Fecha_Envio, Confirmacion_Lectura, IsEdited, EditedAtUtc, IsDeleted
                                                   FROM dbo.Mensaje
                                                   WHERE ID_Chat = @Chat
                                                   ORDER BY Fecha_Envio DESC, ID_Mensaje DESC
                                                   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", cn);
            cmd.Parameters.AddWithValue("@Chat", idChat);
            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", Math.Max(pageSize, 1));
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var mensajes = new List<Mensaje>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                mensajes.Add(MapMensaje(reader));
            }

            return (IReadOnlyList<Mensaje>)mensajes;
        }, ct);

    public async Task<int> CreateMatchAsync(SqlConnection cn, SqlTransaction? tx, int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default)
    {
        await using var cmd = new SqlCommand(@"INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado)
                                               OUTPUT INSERTED.ID_Match
                                               VALUES (@Emisor, @Receptor, @Estado)", cn, tx);
        cmd.Parameters.AddWithValue("@Emisor", idPerfilEmisor);
        cmd.Parameters.AddWithValue("@Receptor", idPerfilReceptor);
        cmd.Parameters.AddWithValue("@Estado", estado);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    public async Task<int> EnsureChatForMatchAsync(SqlConnection cn, SqlTransaction? tx, int idMatch, CancellationToken ct = default)
    {
        await using var selectCmd = new SqlCommand("SELECT ID_Chat FROM dbo.Chat WHERE ID_Match = @Match", cn, tx);
        selectCmd.Parameters.AddWithValue("@Match", idMatch);
        var existing = await selectCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        if (existing is not null)
        {
            return Convert.ToInt32(existing);
        }

        await using var insertCmd = new SqlCommand(@"INSERT INTO dbo.Chat (ID_Match)
                                                     OUTPUT INSERTED.ID_Chat
                                                     VALUES (@Match)", cn, tx);
        insertCmd.Parameters.AddWithValue("@Match", idMatch);
        var created = await insertCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(created);
    }

    public async Task<long> AddMensajeAsync(SqlConnection cn, SqlTransaction? tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default)
    {
        await using var insertCmd = new SqlCommand(@"INSERT INTO dbo.Mensaje
                                                     (ID_Chat, Remitente, Contenido, Confirmacion_Lectura)
                                                     OUTPUT INSERTED.ID_Mensaje, INSERTED.Fecha_Envio
                                                     VALUES (@Chat, @Remitente, @Contenido, @Confirmacion)", cn, tx);
        insertCmd.Parameters.AddWithValue("@Chat", idChat);
        insertCmd.Parameters.AddWithValue("@Remitente", idRemitentePerfil);
        insertCmd.Parameters.AddWithValue("@Contenido", contenido);
        insertCmd.Parameters.AddWithValue("@Confirmacion", confirmacionLectura);
        await using var reader = await insertCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            throw new InvalidOperationException("No se pudo insertar el mensaje");
        }

        var idMensaje = reader.GetInt64(0);
        var fecha = reader.GetDateTime(1);
        await reader.DisposeAsync().ConfigureAwait(false);

        await using var updateCmd = new SqlCommand(@"UPDATE dbo.Chat
                                                     SET LastMessageAtUtc = @Fecha,
                                                         LastMessageId = @IdMensaje
                                                     WHERE ID_Chat = @Chat", cn, tx);
        updateCmd.Parameters.AddWithValue("@Fecha", fecha);
        updateCmd.Parameters.AddWithValue("@IdMensaje", idMensaje);
        updateCmd.Parameters.AddWithValue("@Chat", idChat);
        await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return idMensaje;
    }

    private static Match MapMatch(SqlDataReader reader)
        => new()
        {
            ID_Match = reader.GetInt32(0),
            Perfil_Emisor = reader.GetInt32(1),
            Perfil_Receptor = reader.GetInt32(2),
            Estado = reader.GetString(3).Trim(),
            Fecha_Match = reader.GetDateTime(4)
        };

    private static Chat MapChat(SqlDataReader reader)
        => new()
        {
            ID_Chat = reader.GetInt32(0),
            ID_Match = reader.GetInt32(1),
            Fecha_Creacion = reader.GetDateTime(2),
            LastMessageAtUtc = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
            LastMessageId = reader.IsDBNull(4) ? null : reader.GetInt64(4)
        };

    private static Mensaje MapMensaje(SqlDataReader reader)
        => new()
        {
            ID_Mensaje = reader.GetInt64(0),
            ID_Chat = reader.GetInt32(1),
            Remitente = reader.GetInt32(2),
            Contenido = reader.GetString(3),
            Fecha_Envio = reader.GetDateTime(4),
            Confirmacion_Lectura = reader.GetBoolean(5),
            IsEdited = reader.GetBoolean(6),
            EditedAtUtc = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            IsDeleted = reader.GetBoolean(8)
        };
}

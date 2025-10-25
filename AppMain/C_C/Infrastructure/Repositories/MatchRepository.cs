using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using C_C_Final.Application.Repositories;
using C_C_Final.Domain.Models;
using C_C_Final.Infrastructure.Data;

namespace C_C_Final.Infrastructure.Repositories
{
    public sealed class MatchRepository : RepositoryBase, IMatchRepository
    {
        public MatchRepository(SqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Task<Match?> GetByIdAsync(int idMatch, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match FROM dbo.Match WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    return null;
                }

                return MapMatch(reader);
            }, ct);
        }

        public Task<bool> ExistsAsync(int idPerfilA, int idPerfilB, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = @"SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.Match
    WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B)
       OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)
) THEN 1 ELSE 0 END";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@A", idPerfilA, SqlDbType.Int);
                AddParameter(command, "@B", idPerfilB, SqlDbType.Int);

                var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return Convert.ToInt32(result) == 1;
            }, ct);
        }

        public Task<IReadOnlyList<Match>> ListByPerfilAsync(int idPerfil, int page, int pageSize, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = @"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match
FROM dbo.Match
WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil
ORDER BY Fecha_Match DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Perfil", idPerfil, SqlDbType.Int);
                AddParameter(command, "@Offset", Math.Max(page, 0) * pageSize, SqlDbType.Int);
                AddParameter(command, "@PageSize", pageSize, SqlDbType.Int);

                var list = new List<Match>();
                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    list.Add(MapMatch(reader));
                }

                return (IReadOnlyList<Match>)list;
            }, ct);
        }

        public Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default)
        {
            return WithConnectionAsync(connection => CreateMatchAsync(connection, null, idPerfilEmisor, idPerfilReceptor, estado, ct), ct);
        }

        public Task<bool> UpdateEstadoAsync(int idMatch, string nuevoEstado, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "UPDATE dbo.Match SET Estado = @Estado WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Estado", nuevoEstado ?? string.Empty, SqlDbType.Char, 10);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return rows > 0;
            }, ct);
        }

        public Task<bool> DeleteMatchAsync(int idMatch, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "DELETE FROM dbo.Match WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                var rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return rows > 0;
            }, ct);
        }

        public Task<int> EnsureChatForMatchAsync(int idMatch, CancellationToken ct = default)
        {
            return WithConnectionAsync(connection => EnsureChatForMatchAsync(connection, null, idMatch, ct), ct);
        }

        public Task<Chat?> GetChatByMatchIdAsync(int idMatch, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = "SELECT ID_Chat, ID_Match, Fecha_Creacion, LastMessageAtUtc, LastMessageId FROM dbo.Chat WHERE ID_Match = @Match";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Match", idMatch, SqlDbType.Int);

                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    return null;
                }

                return MapChat(reader);
            }, ct);
        }

        public Task<long> AddMensajeAsync(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default)
        {
            return WithConnectionAsync(connection => AddMensajeAsync(connection, null, idChat, idRemitentePerfil, contenido, confirmacionLectura, ct), ct);
        }

        public Task<IReadOnlyList<Mensaje>> ListMensajesAsync(int idChat, int page, int pageSize, CancellationToken ct = default)
        {
            return WithConnectionAsync(async connection =>
            {
                const string sql = @"SELECT ID_Mensaje, ID_Chat, Remitente, Contenido, Fecha_Envio, Confirmacion_Lectura, IsEdited, EditedAtUtc, IsDeleted
FROM dbo.Mensaje
WHERE ID_Chat = @Chat
ORDER BY Fecha_Envio DESC, ID_Mensaje DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Chat", idChat, SqlDbType.Int);
                AddParameter(command, "@Offset", Math.Max(page, 0) * pageSize, SqlDbType.Int);
                AddParameter(command, "@PageSize", pageSize, SqlDbType.Int);

                var list = new List<Mensaje>();
                using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    list.Add(MapMensaje(reader));
                }

                return (IReadOnlyList<Mensaje>)list;
            }, ct);
        }

        public async Task<int> CreateMatchAsync(SqlConnection connection, SqlTransaction? tx, int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match)
OUTPUT INSERTED.ID_Match
VALUES (@Emisor, @Receptor, @Estado, SYSUTCDATETIME());";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Emisor", idPerfilEmisor, SqlDbType.Int);
            AddParameter(command, "@Receptor", idPerfilReceptor, SqlDbType.Int);
            AddParameter(command, "@Estado", estado ?? string.Empty, SqlDbType.Char, 10);

            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return Convert.ToInt32(result);
        }

        public async Task<int> EnsureChatForMatchAsync(SqlConnection connection, SqlTransaction? tx, int idMatch, CancellationToken ct = default)
        {
            const string sql = @"DECLARE @Existing INT;
SELECT @Existing = ID_Chat FROM dbo.Chat WITH (UPDLOCK, HOLDLOCK) WHERE ID_Match = @Match;
IF @Existing IS NOT NULL
BEGIN
    SELECT @Existing;
END
ELSE
BEGIN
    INSERT INTO dbo.Chat (ID_Match, Fecha_Creacion, LastMessageAtUtc, LastMessageId)
    OUTPUT INSERTED.ID_Chat
    VALUES (@Match, SYSUTCDATETIME(), NULL, NULL);
END";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Match", idMatch, SqlDbType.Int);

            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return Convert.ToInt32(result);
        }

        public async Task<long> AddMensajeAsync(SqlConnection connection, SqlTransaction? tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default)
        {
            var fechaEnvio = DateTime.UtcNow;
            const string insertSql = @"INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Fecha_Envio, Confirmacion_Lectura, IsEdited, EditedAtUtc, IsDeleted)
OUTPUT INSERTED.ID_Mensaje
VALUES (@Chat, @Remitente, @Contenido, @Fecha, @Confirmado, 0, NULL, 0);";
            using var insertCommand = CreateCommand(connection, insertSql, CommandType.Text, tx);
            AddParameter(insertCommand, "@Chat", idChat, SqlDbType.Int);
            AddParameter(insertCommand, "@Remitente", idRemitentePerfil, SqlDbType.Int);
            AddParameter(insertCommand, "@Contenido", contenido ?? string.Empty, SqlDbType.NVarChar, -1);
            AddParameter(insertCommand, "@Fecha", fechaEnvio, SqlDbType.DateTime2);
            AddParameter(insertCommand, "@Confirmado", confirmacionLectura, SqlDbType.Bit);

            var mensajeIdObj = await insertCommand.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var mensajeId = Convert.ToInt64(mensajeIdObj);

            const string updateChatSql = "UPDATE dbo.Chat SET LastMessageAtUtc = @Fecha, LastMessageId = @Mensaje WHERE ID_Chat = @Chat";
            using var updateCommand = CreateCommand(connection, updateChatSql, CommandType.Text, tx);
            AddParameter(updateCommand, "@Fecha", fechaEnvio, SqlDbType.DateTime2);
            AddParameter(updateCommand, "@Mensaje", mensajeId, SqlDbType.BigInt);
            AddParameter(updateCommand, "@Chat", idChat, SqlDbType.Int);
            await updateCommand.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return mensajeId;
        }

        private static Match MapMatch(SqlDataReader reader)
        {
            return new Match
            {
                IdMatch = reader.GetInt32(0),
                PerfilEmisor = reader.GetInt32(1),
                PerfilReceptor = reader.GetInt32(2),
                Estado = reader.GetString(3),
                FechaMatch = reader.GetDateTime(4)
            };
        }

        private static Chat MapChat(SqlDataReader reader)
        {
            return new Chat
            {
                IdChat = reader.GetInt32(0),
                IdMatch = reader.GetInt32(1),
                FechaCreacion = reader.GetDateTime(2),
                LastMessageAtUtc = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                LastMessageId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4)
            };
        }

        private static Mensaje MapMensaje(SqlDataReader reader)
        {
            return new Mensaje
            {
                IdMensaje = reader.GetInt64(0),
                IdChat = reader.GetInt32(1),
                IdRemitentePerfil = reader.GetInt32(2),
                Contenido = reader.GetString(3),
                FechaEnvio = reader.GetDateTime(4),
                ConfirmacionLectura = reader.GetBoolean(5),
                IsEdited = reader.GetBoolean(6),
                EditedAtUtc = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                IsDeleted = reader.GetBoolean(8)
            };
        }
    }
}

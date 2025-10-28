using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    public sealed class MatchRepository : RepositoryBase, IMatchRepository
    {
        public MatchRepository(string connectionString = null) : base(connectionString)
        {
        }

        public Match GetById(int idMatch)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match AS FechaMatch
FROM dbo.Match
WHERE ID_Match = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idMatch, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapMatch(reader);
        }

        public bool Exists(int idPerfilA, int idPerfilB)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.Match
    WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B)
       OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)
) THEN 1 ELSE 0 END";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@A", idPerfilA, SqlDbType.Int);
            AddParameter(command, "@B", idPerfilB, SqlDbType.Int);

            var result = command.ExecuteScalar();
            return SafeToInt32(result) == 1;
        }

        public IReadOnlyList<Match> ListByPerfil(int idPerfil, int page, int pageSize)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match AS FechaMatch
FROM dbo.Match
WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil
ORDER BY Fecha_Match DESC, ID_Match DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Perfil", idPerfil, SqlDbType.Int);
            AddParameter(command, "@Offset", Math.Max(page, 0) * pageSize, SqlDbType.Int);
            AddParameter(command, "@PageSize", pageSize, SqlDbType.Int);

            var list = new List<Match>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapMatch(reader));
            }

            return list;
        }

        public int CreateMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            using var connection = OpenConnection();
            return CreateMatch(connection, null, idPerfilEmisor, idPerfilReceptor, estado);
        }

        public bool UpdateEstado(int idMatch, string nuevoEstado)
        {
            using var connection = OpenConnection();
            const string sql = "UPDATE dbo.Match SET Estado = @Estado WHERE ID_Match = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Estado", nuevoEstado ?? string.Empty, SqlDbType.Char, 10);
            AddParameter(command, "@Id", idMatch, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public bool DeleteMatch(int idMatch)
        {
            using var connection = OpenConnection();
            const string sql = "DELETE FROM dbo.Match WHERE ID_Match = @Id";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Id", idMatch, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        public int EnsureChatForMatch(int idMatch)
        {
            using var connection = OpenConnection();
            return EnsureChatForMatch(connection, null, idMatch);
        }

        public Chat GetChatByMatchId(int idMatch)
        {
            using var connection = OpenConnection();
            const string sql = @"SELECT ID_Chat, ID_Match, Fecha_Creacion AS FechaCreacion, LastMessageAtUtc, LastMessageId
FROM dbo.Chat
WHERE ID_Match = @Match";
            using var command = CreateCommand(connection, sql);
            AddParameter(command, "@Match", idMatch, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapChat(reader);
        }

        public long AddMensaje(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura)
        {
            using var connection = OpenConnection();
            return AddMensaje(connection, null, idChat, idRemitentePerfil, contenido, confirmacionLectura);
        }

        public IReadOnlyList<Mensaje> ListMensajes(int idChat, int page, int pageSize)
        {
            using var connection = OpenConnection();
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
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapMensaje(reader));
            }

            return list;
        }

        public int CreateMatch(SqlConnection connection, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            const string sql = @"INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match)
OUTPUT INSERTED.ID_Match
VALUES (@Emisor, @Receptor, @Estado, SYSUTCDATETIME());";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Emisor", idPerfilEmisor, SqlDbType.Int);
            AddParameter(command, "@Receptor", idPerfilReceptor, SqlDbType.Int);
            AddParameter(command, "@Estado", estado ?? string.Empty, SqlDbType.Char, 10);

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public int EnsureChatForMatch(SqlConnection connection, SqlTransaction tx, int idMatch)
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

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public long AddMensaje(SqlConnection connection, SqlTransaction tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura)
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

            var mensajeIdObj = insertCommand.ExecuteScalar();
            var mensajeId = SafeToInt64(mensajeIdObj);

            const string updateChatSql = "UPDATE dbo.Chat SET LastMessageAtUtc = @Fecha, LastMessageId = @Mensaje WHERE ID_Chat = @Chat";
            using var updateCommand = CreateCommand(connection, updateChatSql, CommandType.Text, tx);
            AddParameter(updateCommand, "@Fecha", fechaEnvio, SqlDbType.DateTime2);
            AddParameter(updateCommand, "@Mensaje", mensajeId, SqlDbType.BigInt);
            AddParameter(updateCommand, "@Chat", idChat, SqlDbType.Int);
            updateCommand.ExecuteNonQuery();

            return mensajeId;
        }

        private static Match MapMatch(SqlDataReader reader)
        {
            var idMatchIndex = reader.GetOrdinal("ID_Match");
            var perfilEmisorIndex = reader.GetOrdinal("Perfil_Emisor");
            var perfilReceptorIndex = reader.GetOrdinal("Perfil_Receptor");
            var estadoIndex = reader.GetOrdinal("Estado");
            var fechaIndex = reader.GetOrdinal("FechaMatch");

            return new Match
            {
                IdMatch = reader.IsDBNull(idMatchIndex) ? 0 : reader.GetInt32(idMatchIndex),
                PerfilEmisor = reader.IsDBNull(perfilEmisorIndex) ? 0 : reader.GetInt32(perfilEmisorIndex),
                PerfilReceptor = reader.IsDBNull(perfilReceptorIndex) ? 0 : reader.GetInt32(perfilReceptorIndex),
                Estado = reader.IsDBNull(estadoIndex) ? string.Empty : reader.GetString(estadoIndex),
                FechaMatch = reader.IsDBNull(fechaIndex) ? DateTime.MinValue : reader.GetDateTime(fechaIndex)
            };
        }

        private static Chat MapChat(SqlDataReader reader)
        {
            var idChatIndex = reader.GetOrdinal("ID_Chat");
            var idMatchIndex = reader.GetOrdinal("ID_Match");
            var fechaIndex = reader.GetOrdinal("FechaCreacion");
            var lastMessageAtIndex = reader.GetOrdinal("LastMessageAtUtc");
            var lastMessageIdIndex = reader.GetOrdinal("LastMessageId");

            return new Chat
            {
                IdChat = reader.IsDBNull(idChatIndex) ? 0 : reader.GetInt32(idChatIndex),
                IdMatch = reader.IsDBNull(idMatchIndex) ? 0 : reader.GetInt32(idMatchIndex),
                FechaCreacion = reader.IsDBNull(fechaIndex) ? DateTime.MinValue : reader.GetDateTime(fechaIndex),
                LastMessageAtUtc = reader.IsDBNull(lastMessageAtIndex) ? (DateTime?)null : reader.GetDateTime(lastMessageAtIndex),
                LastMessageId = reader.IsDBNull(lastMessageIdIndex) ? (long?)null : reader.GetInt64(lastMessageIdIndex)
            };
        }

        private static Mensaje MapMensaje(SqlDataReader reader)
        {
            return new Mensaje
            {
                IdMensaje = reader.IsDBNull(0) ? 0L : reader.GetInt64(0),
                IdChat = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                IdRemitentePerfil = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                Contenido = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                FechaEnvio = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                ConfirmacionLectura = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                IsEdited = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                EditedAtUtc = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                IsDeleted = reader.IsDBNull(8) ? false : reader.GetBoolean(8)
            };
        }
    }
}

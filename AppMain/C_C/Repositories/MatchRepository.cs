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
            return WithConnection(connection =>
            {
                var fechaColumn = ResolveMatchFechaColumn(connection, null);
                var selectColumns = BuildMatchSelectColumns(fechaColumn);
                var sql = $"SELECT {selectColumns} FROM dbo.Match WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return MapMatch(reader);
            });
        }

        public bool Exists(int idPerfilA, int idPerfilB)
        {
            return WithConnection(connection =>
            {
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
            });
        }

        public IReadOnlyList<Match> ListByPerfil(int idPerfil, int page, int pageSize)
        {
            return WithConnection(connection =>
            {
                var fechaColumn = ResolveMatchFechaColumn(connection, null);
                var selectColumns = BuildMatchSelectColumns(fechaColumn);
                var orderBy = !string.IsNullOrEmpty(fechaColumn)
                    ? $"ORDER BY {WrapColumn(fechaColumn)} DESC"
                    : "ORDER BY ID_Match DESC";

                var sql = $@"SELECT {selectColumns}
FROM dbo.Match
WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil
{orderBy}
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

                return (IReadOnlyList<Match>)list;
            });
        }

        public int CreateMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            return WithConnection(connection => CreateMatch(connection, null, idPerfilEmisor, idPerfilReceptor, estado));
        }

        public bool UpdateEstado(int idMatch, string nuevoEstado)
        {
            return WithConnection(connection =>
            {
                const string sql = "UPDATE dbo.Match SET Estado = @Estado WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Estado", nuevoEstado ?? string.Empty, SqlDbType.Char, 10);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                var rows = command.ExecuteNonQuery();
                return rows > 0;
            });
        }

        public bool DeleteMatch(int idMatch)
        {
            return WithConnection(connection =>
            {
                const string sql = "DELETE FROM dbo.Match WHERE ID_Match = @Id";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Id", idMatch, SqlDbType.Int);

                var rows = command.ExecuteNonQuery();
                return rows > 0;
            });
        }

        public int EnsureChatForMatch(int idMatch)
        {
            return WithConnection(connection => EnsureChatForMatch(connection, null, idMatch));
        }

        public Chat GetChatByMatchId(int idMatch)
        {
            return WithConnection(connection =>
            {
                var fechaColumn = ResolveChatFechaColumn(connection, null);
                var selectColumns = BuildChatSelectColumns(fechaColumn);
                var sql = $"SELECT {selectColumns} FROM dbo.Chat WHERE ID_Match = @Match";
                using var command = CreateCommand(connection, sql);
                AddParameter(command, "@Match", idMatch, SqlDbType.Int);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return MapChat(reader);
            });
        }

        public long AddMensaje(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura)
        {
            return WithConnection(connection => AddMensaje(connection, null, idChat, idRemitentePerfil, contenido, confirmacionLectura));
        }

        public IReadOnlyList<Mensaje> ListMensajes(int idChat, int page, int pageSize)
        {
            return WithConnection(connection =>
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
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapMensaje(reader));
                }

                return (IReadOnlyList<Mensaje>)list;
            });
        }

        public int CreateMatch(SqlConnection connection, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            var fechaColumn = ResolveMatchFechaColumn(connection, tx);
            var insertColumns = new List<string>
            {
                "Perfil_Emisor",
                "Perfil_Receptor",
                "Estado"
            };

            var insertValues = new List<string>
            {
                "@Emisor",
                "@Receptor",
                "@Estado"
            };

            if (!string.IsNullOrEmpty(fechaColumn))
            {
                insertColumns.Add(WrapColumn(fechaColumn));
                insertValues.Add("SYSUTCDATETIME()");
            }

            var sql = $@"INSERT INTO dbo.Match ({string.Join(", ", insertColumns)})
OUTPUT INSERTED.ID_Match
VALUES ({string.Join(", ", insertValues)});";
            using var command = CreateCommand(connection, sql, CommandType.Text, tx);
            AddParameter(command, "@Emisor", idPerfilEmisor, SqlDbType.Int);
            AddParameter(command, "@Receptor", idPerfilReceptor, SqlDbType.Int);
            AddParameter(command, "@Estado", estado ?? string.Empty, SqlDbType.Char, 10);

            var result = command.ExecuteScalar();
            return SafeToInt32(result);
        }

        public int EnsureChatForMatch(SqlConnection connection, SqlTransaction tx, int idMatch)
        {
            var fechaColumn = ResolveChatFechaColumn(connection, tx);
            var insertColumns = new List<string> { "ID_Match" };
            var insertValues = new List<string> { "@Match" };

            if (!string.IsNullOrEmpty(fechaColumn))
            {
                insertColumns.Add(WrapColumn(fechaColumn));
                insertValues.Add("SYSUTCDATETIME()");
            }

            insertColumns.Add("LastMessageAtUtc");
            insertColumns.Add("LastMessageId");
            insertValues.Add("NULL");
            insertValues.Add("NULL");

            var sql = $@"DECLARE @Existing INT;
SELECT @Existing = ID_Chat FROM dbo.Chat WITH (UPDLOCK, HOLDLOCK) WHERE ID_Match = @Match;
IF @Existing IS NOT NULL
BEGIN
    SELECT @Existing;
END
ELSE
BEGIN
    INSERT INTO dbo.Chat ({string.Join(", ", insertColumns)})
    OUTPUT INSERTED.ID_Chat
    VALUES ({string.Join(", ", insertValues)});
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

        private string ResolveMatchFechaColumn(SqlConnection connection, SqlTransaction tx)
        {
            if (ColumnExists(connection, tx, "Match", "Fecha_Match"))
            {
                return "Fecha_Match";
            }

            if (ColumnExists(connection, tx, "Match", "FechaMatch"))
            {
                return "FechaMatch";
            }

            return null;
        }

        private string ResolveChatFechaColumn(SqlConnection connection, SqlTransaction tx)
        {
            if (ColumnExists(connection, tx, "Chat", "Fecha_Creacion"))
            {
                return "Fecha_Creacion";
            }

            if (ColumnExists(connection, tx, "Chat", "FechaCreacion"))
            {
                return "FechaCreacion";
            }

            return null;
        }

        private static string BuildMatchSelectColumns(string fechaColumn)
        {
            var columns = new List<string>
            {
                "ID_Match",
                "Perfil_Emisor",
                "Perfil_Receptor",
                "Estado"
            };

            columns.Add(!string.IsNullOrEmpty(fechaColumn)
                ? $"{WrapColumn(fechaColumn)} AS FechaMatch"
                : "CAST('1900-01-01T00:00:00' AS datetime2(0)) AS FechaMatch");

            return string.Join(", ", columns);
        }

        private static string BuildChatSelectColumns(string fechaColumn)
        {
            var columns = new List<string>
            {
                "ID_Chat",
                "ID_Match"
            };

            columns.Add(!string.IsNullOrEmpty(fechaColumn)
                ? $"{WrapColumn(fechaColumn)} AS FechaCreacion"
                : "CAST('1900-01-01T00:00:00' AS datetime2(0)) AS FechaCreacion");
            columns.Add("LastMessageAtUtc");
            columns.Add("LastMessageId");

            return string.Join(", ", columns);
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

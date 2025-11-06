using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using C_C_Final.Model;

namespace C_C_Final.Repositories
{
    /// <summary>
    /// Implementa las operaciones de persistencia para emparejamientos, chats y mensajes.
    /// </summary>
    public sealed class MatchRepository : RepositoryBase, IMatchRepository
    {
        /// <summary>
        ///     Permite reutilizar la lógica base del repositorio con una cadena de conexión específica.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión opcional.</param>
        public MatchRepository(string connectionString = null) : base(connectionString)
        {
        }

        /// <inheritdoc />
        public Match ObtenerPorId(int idMatch)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match AS FechaMatch
FROM dbo.Match
WHERE ID_Match = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idMatch, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearMatch(reader);
        }

        /// <inheritdoc />
        public Match ObtenerPorPerfiles(int idPerfilA, int idPerfilB)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT TOP 1 ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match AS FechaMatch
FROM dbo.Match
WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B)
   OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)
ORDER BY Fecha_Match DESC, ID_Match DESC";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@A", idPerfilA, SqlDbType.Int);
            AgregarParametro(command, "@B", idPerfilB, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearMatch(reader);
        }

        /// <inheritdoc />
        public bool Existe(int idPerfilA, int idPerfilB)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.Match
    WHERE (Perfil_Emisor = @A AND Perfil_Receptor = @B)
       OR (Perfil_Emisor = @B AND Perfil_Receptor = @A)
) THEN 1 ELSE 0 END";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@A", idPerfilA, SqlDbType.Int);
            AgregarParametro(command, "@B", idPerfilB, SqlDbType.Int);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result) == 1;
        }

        /// <inheritdoc />
        public IReadOnlyList<Match> ListarPorPerfil(int idPerfil, int page, int pageSize)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Match, Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match AS FechaMatch
FROM dbo.Match
WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil
ORDER BY Fecha_Match DESC, ID_Match DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Perfil", idPerfil, SqlDbType.Int);
            AgregarParametro(command, "@Offset", Math.Max(page, 0) * pageSize, SqlDbType.Int);
            AgregarParametro(command, "@PageSize", pageSize, SqlDbType.Int);

            var list = new List<Match>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapearMatch(reader));
            }

            return list;
        }

        /// <inheritdoc />
        public bool ActualizarEstado(int idMatch, string nuevoEstado)
        {
            using var connection = AbrirConexion();
            const string sql = "UPDATE dbo.Match SET Estado = @Estado WHERE ID_Match = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Estado", nuevoEstado ?? string.Empty, SqlDbType.NVarChar, 10);
            AgregarParametro(command, "@Id", idMatch, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public void ActualizarParticipantes(int idMatch, int nuevoEmisor, int nuevoReceptor)
        {
            if (nuevoEmisor == nuevoReceptor)
            {
                throw new ArgumentException("Los perfiles del match deben ser distintos", nameof(nuevoEmisor));
            }

            using var connection = AbrirConexion();
            const string sql = "UPDATE dbo.Match SET Perfil_Emisor = @Emisor, Perfil_Receptor = @Receptor WHERE ID_Match = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Emisor", nuevoEmisor, SqlDbType.Int);
            AgregarParametro(command, "@Receptor", nuevoReceptor, SqlDbType.Int);
            AgregarParametro(command, "@Id", idMatch, SqlDbType.Int);

            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public bool EliminarMatch(int idMatch)
        {
            using var connection = AbrirConexion();
            const string sql = "DELETE FROM dbo.Match WHERE ID_Match = @Id";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Id", idMatch, SqlDbType.Int);

            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }

        /// <inheritdoc />
        public Chat ObtenerChatPorMatchId(int idMatch)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Chat, ID_Match, Fecha_Creacion AS FechaCreacion, LastMessageAtUtc, LastMessageId
FROM dbo.Chat
WHERE ID_Match = @Match";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Match", idMatch, SqlDbType.Int);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return MapearChat(reader);
        }

        /// <inheritdoc />
        public IReadOnlyList<Mensaje> ListarMensajes(int idChat, int page, int pageSize)
        {
            using var connection = AbrirConexion();
            const string sql = @"SELECT ID_Mensaje, ID_Chat, Remitente, Contenido, Fecha_Envio, Confirmacion_Lectura, IsEdited, EditedAtUtc, IsDeleted
FROM dbo.Mensaje
WHERE ID_Chat = @Chat
ORDER BY Fecha_Envio DESC, ID_Mensaje DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            using var command = CrearComando(connection, sql);
            AgregarParametro(command, "@Chat", idChat, SqlDbType.Int);
            AgregarParametro(command, "@Offset", Math.Max(page, 0) * pageSize, SqlDbType.Int);
            AgregarParametro(command, "@PageSize", pageSize, SqlDbType.Int);

            var list = new List<Mensaje>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapearMensaje(reader));
            }

            return list;
        }

        /// <inheritdoc />
        public int CrearMatch(SqlConnection connection, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            if (idPerfilEmisor == idPerfilReceptor)
            {
                throw new ArgumentException("Los perfiles del match deben ser distintos", nameof(idPerfilEmisor));
            }

            // El match se registra con la marca temporal en UTC para facilitar la sincronización entre clientes.
            const string sql = @"INSERT INTO dbo.Match (Perfil_Emisor, Perfil_Receptor, Estado, Fecha_Match)
OUTPUT INSERTED.ID_Match
VALUES (@Emisor, @Receptor, @Estado, SYSUTCDATETIME());";
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Emisor", idPerfilEmisor, SqlDbType.Int);
            AgregarParametro(command, "@Receptor", idPerfilReceptor, SqlDbType.Int);
            AgregarParametro(command, "@Estado", estado ?? string.Empty, SqlDbType.NVarChar, 10);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <inheritdoc />
        public int AsegurarChatParaMatch(SqlConnection connection, SqlTransaction tx, int idMatch)
        {
            // El bloque T-SQL utiliza bloqueos de actualización (UPDLOCK, HOLDLOCK) para garantizar que solo
            // se cree un chat por match incluso cuando múltiples hilos/usuarios intenten hacerlo a la vez.
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
            using var command = CrearComando(connection, sql, CommandType.Text, tx);
            AgregarParametro(command, "@Match", idMatch, SqlDbType.Int);

            var result = command.ExecuteScalar();
            return ConvertirSeguroAInt32(result);
        }

        /// <inheritdoc />
        public long AgregarMensaje(SqlConnection connection, SqlTransaction tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura)
        {
            var fechaEnvio = DateTime.UtcNow;
            // La inserción retorna el identificador del nuevo mensaje para actualizar la cabecera del chat
            // inmediatamente después dentro de la misma transacción.
            const string insertSql = @"INSERT INTO dbo.Mensaje (ID_Chat, Remitente, Contenido, Fecha_Envio, Confirmacion_Lectura, IsEdited, EditedAtUtc, IsDeleted)
OUTPUT INSERTED.ID_Mensaje
VALUES (@Chat, @Remitente, @Contenido, @Fecha, @Confirmado, 0, NULL, 0);";
            using var insertCommand = CrearComando(connection, insertSql, CommandType.Text, tx);
            AgregarParametro(insertCommand, "@Chat", idChat, SqlDbType.Int);
            AgregarParametro(insertCommand, "@Remitente", idRemitentePerfil, SqlDbType.Int);
            AgregarParametro(insertCommand, "@Contenido", contenido ?? string.Empty, SqlDbType.NVarChar, -1);
            AgregarParametro(insertCommand, "@Fecha", fechaEnvio, SqlDbType.DateTime2);
            AgregarParametro(insertCommand, "@Confirmado", confirmacionLectura, SqlDbType.Bit);

            var mensajeIdObj = insertCommand.ExecuteScalar();
            var mensajeId = ConvertirSeguroAInt64(mensajeIdObj);

            // Tras insertar el mensaje se actualiza la metadata del chat para que la UI pueda ordenar las
            // conversaciones por actividad reciente sin ejecutar otra consulta.
            const string updateChatSql = "UPDATE dbo.Chat SET LastMessageAtUtc = @Fecha, LastMessageId = @Mensaje WHERE ID_Chat = @Chat";
            using var updateCommand = CrearComando(connection, updateChatSql, CommandType.Text, tx);
            AgregarParametro(updateCommand, "@Fecha", fechaEnvio, SqlDbType.DateTime2);
            AgregarParametro(updateCommand, "@Mensaje", mensajeId, SqlDbType.BigInt);
            AgregarParametro(updateCommand, "@Chat", idChat, SqlDbType.Int);
            updateCommand.ExecuteNonQuery();

            return mensajeId;
        }

        public void EliminarMatchesPorPerfil(int idPerfil, SqlConnection connection, SqlTransaction transaction)
        {
            const string sql = @"DELETE FROM dbo.Match 
                         WHERE Perfil_Emisor = @IdPerfil OR Perfil_Receptor = @IdPerfil";

            using var command = CrearComando(connection, sql, CommandType.Text, transaction);
            AgregarParametro(command, "@IdPerfil", idPerfil, SqlDbType.Int);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Convierte un registro en una entidad de emparejamiento.
        /// </summary>
        /// <param name="reader">Lector con los datos del emparejamiento.</param>
        /// <returns>Instancia de <see cref="Match"/>.</returns>
        private static Match MapearMatch(SqlDataReader reader)
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

        /// <summary>
        /// Convierte un registro en una entidad de chat.
        /// </summary>
        /// <param name="reader">Lector con los datos del chat.</param>
        /// <returns>Instancia de <see cref="Chat"/>.</returns>
        private static Chat MapearChat(SqlDataReader reader)
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

        /// <summary>
        /// Convierte un registro en una entidad de mensaje.
        /// </summary>
        /// <param name="reader">Lector con los datos del mensaje.</param>
        /// <returns>Instancia de <see cref="Mensaje"/>.</returns>
        private static Mensaje MapearMensaje(SqlDataReader reader)
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

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using C_C_Final.Repositories;

namespace C_C_Final.Services
{
    /// <summary>
    /// Elimina de forma integral una cuenta y todas sus relaciones en la base de datos.
    /// </summary>
    public sealed class CuentaDeletionService
    {
        private readonly string _connectionString;

        public CuentaDeletionService(string connectionString = null)
        {
            _connectionString = RepositoryBase.ResolverCadenaConexion(connectionString);
        }

        /// <summary>
        /// Elimina la cuenta, el alumno, perfiles, preferencias, chats, mensajes y matches asociados.
        /// </summary>
        /// <param name="cuentaId">Identificador de la cuenta a eliminar.</param>
        public void EliminarCuentaCompleta(int cuentaId)
        {
            if (cuentaId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cuentaId), "El identificador de la cuenta debe ser mayor que cero.");
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (!CuentaExiste(connection, transaction, cuentaId))
                {
                    throw new InvalidOperationException("La cuenta indicada no existe.");
                }

                var perfiles = ObtenerIds(connection, transaction, "SELECT ID_Perfil FROM dbo.Perfil WHERE ID_Cuenta = @Id", cuentaId);
                var matchIds = ObtenerMatches(connection, transaction, perfiles);
                var chatIds = ObtenerChats(connection, transaction, matchIds);

                EliminarMensajes(connection, transaction, chatIds);
                EliminarChats(connection, transaction, chatIds);
                EliminarMatches(connection, transaction, matchIds);
                EliminarPreferencias(connection, transaction, perfiles);
                EliminarPerfiles(connection, transaction, perfiles);
                EliminarAlumno(connection, transaction, cuentaId);
                EliminarCuenta(connection, transaction, cuentaId);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static bool CuentaExiste(SqlConnection connection, SqlTransaction transaction, int cuentaId)
        {
            using var command = new SqlCommand("SELECT COUNT(1) FROM dbo.Cuenta WHERE ID_Cuenta = @Id", connection, transaction);
            command.Parameters.AddWithValue("@Id", cuentaId);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        private static List<int> ObtenerIds(SqlConnection connection, SqlTransaction transaction, string query, int cuentaId)
        {
            var ids = new List<int>();
            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@Id", cuentaId);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ids.Add(reader.IsDBNull(0) ? 0 : reader.GetInt32(0));
                }
            }
            return ids;
        }

        private static HashSet<int> ObtenerMatches(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> perfiles)
        {
            var matchIds = new HashSet<int>();
            using var command = new SqlCommand("SELECT ID_Match FROM dbo.Match WHERE Perfil_Emisor = @Perfil OR Perfil_Receptor = @Perfil", connection, transaction);
            var parametroPerfil = command.Parameters.Add("@Perfil", System.Data.SqlDbType.Int);

            foreach (var perfil in perfiles)
            {
                if (perfil <= 0)
                {
                    continue;
                }

                parametroPerfil.Value = perfil;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var matchId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        if (matchId > 0)
                        {
                            matchIds.Add(matchId);
                        }
                    }
                }
            }

            return matchIds;
        }

        private static HashSet<int> ObtenerChats(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> matches)
        {
            var chatIds = new HashSet<int>();
            using var command = new SqlCommand("SELECT ID_Chat FROM dbo.Chat WHERE ID_Match = @Match", connection, transaction);
            var parametroMatch = command.Parameters.Add("@Match", System.Data.SqlDbType.Int);

            foreach (var matchId in matches)
            {
                if (matchId <= 0)
                {
                    continue;
                }

                parametroMatch.Value = matchId;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var chatId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        if (chatId > 0)
                        {
                            chatIds.Add(chatId);
                        }
                    }
                }
            }

            return chatIds;
        }

        private static void EliminarMensajes(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> chatIds)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Mensaje WHERE ID_Chat = @Chat", connection, transaction);
            var parametroChat = command.Parameters.Add("@Chat", System.Data.SqlDbType.Int);

            foreach (var chatId in chatIds)
            {
                if (chatId <= 0)
                {
                    continue;
                }

                parametroChat.Value = chatId;
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarChats(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> chatIds)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Chat WHERE ID_Chat = @Chat", connection, transaction);
            var parametroChat = command.Parameters.Add("@Chat", System.Data.SqlDbType.Int);

            foreach (var chatId in chatIds)
            {
                if (chatId <= 0)
                {
                    continue;
                }

                parametroChat.Value = chatId;
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarMatches(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> matchIds)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Match WHERE ID_Match = @Match", connection, transaction);
            var parametroMatch = command.Parameters.Add("@Match", System.Data.SqlDbType.Int);

            foreach (var matchId in matchIds)
            {
                if (matchId <= 0)
                {
                    continue;
                }

                parametroMatch.Value = matchId;
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarPreferencias(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> perfiles)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Preferencias WHERE ID_Perfil = @Perfil", connection, transaction);
            var parametroPerfil = command.Parameters.Add("@Perfil", System.Data.SqlDbType.Int);

            foreach (var perfil in perfiles)
            {
                if (perfil <= 0)
                {
                    continue;
                }

                parametroPerfil.Value = perfil;
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarPerfiles(SqlConnection connection, SqlTransaction transaction, IEnumerable<int> perfiles)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Perfil WHERE ID_Perfil = @Perfil", connection, transaction);
            var parametroPerfil = command.Parameters.Add("@Perfil", System.Data.SqlDbType.Int);

            foreach (var perfil in perfiles)
            {
                if (perfil <= 0)
                {
                    continue;
                }

                parametroPerfil.Value = perfil;
                command.ExecuteNonQuery();
            }
        }

        private static void EliminarAlumno(SqlConnection connection, SqlTransaction transaction, int cuentaId)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Alumno WHERE ID_Cuenta = @Cuenta", connection, transaction);
            command.Parameters.AddWithValue("@Cuenta", cuentaId);
            command.ExecuteNonQuery();
        }

        private static void EliminarCuenta(SqlConnection connection, SqlTransaction transaction, int cuentaId)
        {
            using var command = new SqlCommand("DELETE FROM dbo.Cuenta WHERE ID_Cuenta = @Cuenta", connection, transaction);
            command.Parameters.AddWithValue("@Cuenta", cuentaId);
            var affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                throw new InvalidOperationException("No se pudo eliminar la cuenta especificada.");
            }
        }
    }
}
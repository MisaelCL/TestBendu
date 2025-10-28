using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace C_C_Final.Repositories
{
    public abstract class RepositoryBase
    {
        protected const int DefaultCommandTimeout = 30;

        private readonly string _connectionString;
        private static string _cachedConnectionString;

        protected RepositoryBase(string connectionString = null)
        {
            _connectionString = ResolveConnectionString(connectionString);
        }

        internal static string ResolveConnectionString(string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return NormalizeConnectionString(connectionString, "proporcionada");
            }

            if (!string.IsNullOrEmpty(_cachedConnectionString))
            {
                return _cachedConnectionString;
            }

            var configured = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            _cachedConnectionString = NormalizeConnectionString(configured, "DefaultConnection");
            return _cachedConnectionString;
        }

        protected SqlCommand CreateCommand(SqlConnection connection, string sql, CommandType commandType = CommandType.Text, SqlTransaction transaction = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = commandType;
            command.CommandTimeout = DefaultCommandTimeout;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            return command;
        }

        protected static void AddParameter(SqlCommand command, string name, object value, SqlDbType? type = null, int? size = null)
        {
            var parameter = command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            if (type.HasValue)
            {
                parameter.SqlDbType = type.Value;
            }

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }
        }

        protected static int SafeToInt32(object result)
        {
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        protected static long SafeToInt64(object result)
        {
            return result == null || result == DBNull.Value ? 0L : Convert.ToInt64(result);
        }

        protected SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static string NormalizeConnectionString(string connectionString, string sourceName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"No se encontró la cadena de conexión '{sourceName}'.");
            }

            SqlConnectionStringBuilder builder;
            try
            {
                builder = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"La cadena de conexión '{sourceName}' no es válida.", ex);
            }

            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                throw new InvalidOperationException($"La cadena de conexión '{sourceName}' debe especificar la base de datos mediante 'Initial Catalog'.");
            }

            return builder.ConnectionString;
        }
    }
}

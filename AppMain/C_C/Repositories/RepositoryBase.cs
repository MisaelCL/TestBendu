using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace C_C_Final.Repositories
{
    public abstract class RepositoryBase
    {
        protected const int DefaultCommandTimeout = 30;

        private readonly string _connectionString;
        private static readonly Lazy<string> CachedConnectionString = new Lazy<string>(
            () => NormalizeConnectionString(
                ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString,
                "DefaultConnection"),
            LazyThreadSafetyMode.ExecutionAndPublication);

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

            return CachedConnectionString.Value;
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

        protected bool ColumnExists(SqlConnection connection, SqlTransaction transaction, string tableName, string columnName)
        {
            const string sql = @"SELECT 1
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = @Table
  AND COLUMN_NAME = @Column";

            using var command = CreateCommand(connection, sql, CommandType.Text, transaction);
            AddParameter(command, "@Table", tableName, SqlDbType.NVarChar, 128);
            AddParameter(command, "@Column", columnName, SqlDbType.NVarChar, 128);

            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        protected static string WrapColumn(string columnName)
        {
            return string.IsNullOrWhiteSpace(columnName) || columnName.Contains("[", StringComparison.Ordinal)
                ? columnName
                : $"[{columnName}]";
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

        protected T WithConnection<T>(Func<SqlConnection, T> action)
        {
            using var connection = OpenConnection();
            return action(connection);
        }

        protected void WithConnection(Action<SqlConnection> action)
        {
            using var connection = OpenConnection();
            action(connection);
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

using System;
using System.Data;
using System.Data.SqlClient;

namespace C_C_Final.Repositories
{
    public abstract class RepositoryBase
    {
        protected const int DefaultCommandTimeout = 30;

        protected RepositoryBase(SqlConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        protected SqlConnectionFactory ConnectionFactory { get; }

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
            var connection = ConnectionFactory.CreateConnection();
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
    }
}

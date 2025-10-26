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

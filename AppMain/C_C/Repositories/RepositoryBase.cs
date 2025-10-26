using System;

using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

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

        protected async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct)
        {
            var connection = ConnectionFactory.CreateConnection();
            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
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

        protected async Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> action, CancellationToken ct)
        {
            var connection = await OpenConnectionAsync(ct).ConfigureAwait(false);
            return await action(connection).ConfigureAwait(false);
        }

        protected async Task WithConnectionAsync(Func<SqlConnection, Task> action, CancellationToken ct)
        {
            var connection = await OpenConnectionAsync(ct).ConfigureAwait(false);
            await action(connection).ConfigureAwait(false);
        }
    }
}

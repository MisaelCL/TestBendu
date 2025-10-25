using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;
using C_C.Resources.utils;

namespace C_C.Repositories;

public abstract class RepositoryBase
{
    private readonly IConnectionFactory _connectionFactory;

    protected RepositoryBase(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected async Task<TResult> WithConnectionAsync<TResult>(Func<SqlConnection, Task<TResult>> action, CancellationToken ct)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return await action(connection).ConfigureAwait(false);
    }

    protected SqlParameter P(string name, object? value, SqlDbType? type = null)
    {
        var parameter = new SqlParameter(name, value ?? DBNull.Value);
        if (type.HasValue)
        {
            parameter.SqlDbType = type.Value;
        }

        return parameter;
    }
}

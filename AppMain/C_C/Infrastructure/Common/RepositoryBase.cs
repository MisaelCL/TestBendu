using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Common;

public abstract class RepositoryBase
{
    private readonly SqlConnectionFactory _connectionFactory;

    protected RepositoryBase(SqlConnectionFactory? connectionFactory = null)
    {
        _connectionFactory = connectionFactory ?? new SqlConnectionFactory();
    }

    protected SqlConnection CreateConnection() => _connectionFactory.CreateConnection();

    protected async Task WithConnectionAsync(Func<SqlConnection, Task> action, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        await action(connection).ConfigureAwait(false);
    }

    protected async Task<TResult> WithConnectionAsync<TResult>(Func<SqlConnection, Task<TResult>> action, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return await action(connection).ConfigureAwait(false);
    }
}

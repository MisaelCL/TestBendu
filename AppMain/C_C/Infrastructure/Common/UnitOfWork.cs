using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Common;

public sealed class UnitOfWork : IAsyncDisposable
{
    private readonly SqlConnectionFactory _connectionFactory;

    public UnitOfWork(SqlConnectionFactory? connectionFactory = null)
    {
        _connectionFactory = connectionFactory ?? new SqlConnectionFactory();
    }

    public SqlConnection? Connection { get; private set; }
    public SqlTransaction? Transaction { get; private set; }

    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (Connection is not null)
        {
            throw new InvalidOperationException("La transacción ya fue iniciada.");
        }

        Connection = _connectionFactory.CreateConnection();
        await Connection.OpenAsync(ct).ConfigureAwait(false);
        Transaction = await Connection.BeginTransactionAsync(ct).ConfigureAwait(false);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (Transaction is null)
        {
            throw new InvalidOperationException("No existe una transacción activa para confirmar.");
        }

        await Transaction.CommitAsync(ct).ConfigureAwait(false);
        await DisposeAsync().ConfigureAwait(false);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (Transaction is not null)
        {
            await Transaction.RollbackAsync(ct).ConfigureAwait(false);
        }

        await DisposeAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (Transaction is not null)
        {
            await Transaction.DisposeAsync().ConfigureAwait(false);
            Transaction = null;
        }

        if (Connection is not null)
        {
            await Connection.DisposeAsync().ConfigureAwait(false);
            Connection = null;
        }
    }
}

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace C_C_Final.Infrastructure.Data
{
    public sealed class UnitOfWork : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;
        private bool _disposed;

        private UnitOfWork(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public SqlConnection Connection => _connection;
        public SqlTransaction Transaction => _transaction;

        public static async Task<UnitOfWork> CreateAsync(SqlConnectionFactory factory, CancellationToken ct)
        {
            var connection = factory.CreateConnection();
            await connection.OpenAsync(ct).ConfigureAwait(false);
            var transaction = connection.BeginTransaction();
            return new UnitOfWork(connection, transaction);
        }

        public async Task CommitAsync(CancellationToken ct)
        {
            await _transaction.CommitAsync(ct).ConfigureAwait(false);
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            await _transaction.RollbackAsync(ct).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _transaction.Dispose();
            _connection.Dispose();
            _disposed = true;
        }
    }
}

using System;
using System.Data.SqlClient;

namespace C_C_Final.Repositories
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

        public static UnitOfWork Create(SqlConnectionFactory factory)
        {
            var connection = factory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();
            return new UnitOfWork(connection, transaction);
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
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

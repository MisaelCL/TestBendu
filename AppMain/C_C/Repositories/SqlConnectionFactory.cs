using System;
using System.Configuration;
using System.Data.SqlClient;

namespace C_C_Final.Repositories
{
    public sealed class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory()
        {
            var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en App.config.");
            }

            _connectionString = connection;
        }

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexión no puede ser nula o vacía.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}

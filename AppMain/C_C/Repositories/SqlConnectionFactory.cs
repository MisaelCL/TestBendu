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
            _connectionString = NormalizeConnectionString(connection, "DefaultConnection");
        }

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = NormalizeConnectionString(connectionString, "proporcionada");
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);

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

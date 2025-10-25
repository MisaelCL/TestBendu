using System.Configuration;
using Microsoft.Data.SqlClient;

namespace C_C.Infrastructure.Common;

public sealed class SqlConnectionFactory
{
    public SqlConnectionFactory()
    {
        ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
    }

    public SqlConnectionFactory(string connectionString)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public string ConnectionString { get; }

    public SqlConnection CreateConnection() => new(ConnectionString);
}

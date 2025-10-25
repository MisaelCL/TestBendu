using System.Globalization;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace C_C.Repositories;

public sealed class CuentaRepository : RepositoryBase, ICuentaRepository
{
    private readonly ILogger<CuentaRepository> _logger;

    public CuentaRepository(IConnectionFactory connectionFactory, ILogger<CuentaRepository> logger)
        : base(connectionFactory)
    {
        _logger = logger;
    }

    public Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"SELECT ID_Cuenta, Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro
FROM dbo.Cuenta WHERE Email = @Email";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Email", email));
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return new Cuenta
                {
                    ID_Cuenta = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    Hash_Contrasena = reader.GetString(2),
                    Estado_Cuenta = reader.GetByte(3),
                    Fecha_Registro = reader.GetDateTime(4)
                };
            }

            return null;
        }, ct);
    }

    public Task<int> InsertAsync(Cuenta c, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"INSERT INTO dbo.Cuenta (Email, Hash_Contrasena, Estado_Cuenta, Fecha_Registro)
VALUES (@Email, @Hash, @Estado, @FechaRegistro);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Email", c.Email));
            command.Parameters.Add(P("@Hash", c.Hash_Contrasena));
            command.Parameters.Add(P("@Estado", c.Estado_Cuenta));
            command.Parameters.Add(P("@FechaRegistro", c.Fecha_Registro));

            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var id = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            _logger.LogInformation("Cuenta creada con ID {CuentaId}", id);
            return id;
        }, ct);
    }
}

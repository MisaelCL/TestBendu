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
            const string sql = @"SELECT ID_Cuenta, ID_Alumno, Email, PasswordHash, Fecha_Registro, Ultimo_Acceso, IsActive
FROM dbo.Cuenta WHERE Email = @Email";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Email", email));
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return new Cuenta
                {
                    ID_Cuenta = reader.GetInt32(0),
                    ID_Alumno = reader.GetInt32(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Fecha_Registro = reader.GetDateTime(4),
                    Ultimo_Acceso = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsActive = reader.GetBoolean(6)
                };
            }

            return null;
        }, ct);
    }

    public Task<int> InsertAsync(Cuenta c, CancellationToken ct = default)
    {
        return WithConnectionAsync(async connection =>
        {
            const string sql = @"INSERT INTO dbo.Cuenta (ID_Alumno, Email, PasswordHash, Fecha_Registro, Ultimo_Acceso, IsActive)
VALUES (@Alumno, @Email, @Hash, @FechaRegistro, @UltimoAcceso, @Activo);
SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(P("@Alumno", c.ID_Alumno));
            command.Parameters.Add(P("@Email", c.Email));
            command.Parameters.Add(P("@Hash", c.PasswordHash));
            command.Parameters.Add(P("@FechaRegistro", c.Fecha_Registro));
            command.Parameters.Add(P("@UltimoAcceso", c.Ultimo_Acceso ?? (object)DBNull.Value));
            command.Parameters.Add(P("@Activo", c.IsActive));

            var result = await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var id = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            _logger.LogInformation("Cuenta creada con ID {CuentaId}", id);
            return id;
        }, ct);
    }
}
